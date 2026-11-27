using System.Text.Json;
using FaceRecognitionApp.Models;
using OpenCvSharp;
using OpenCvSharp.Face;

namespace FaceRecognitionApp.Services;

/// <summary>LBPH algoritmasıyla yüz tanır. dataset/ klasöründen eğitir, modeli model.yml, etiketleri labels.json olarak saklar.</summary>
public sealed class FaceRecognizerService : IDisposable
{
    /// <summary>
    /// Varsayılan eşik. LBPH mesafesi bunun üstündeyse yüz "Bilinmiyor" sayılır.
    /// Gerçek dataset üzerindeki leave-one-out değerlendirmesinde iki kişi arasındaki çapraz mesafeler en az 74,9 çıktı;
    /// fotoğraflardaki doğru eşleşmeler ise 66-68 civarında. 70 iki kişiyi karıştırmadan bunları kabul eder.
    /// Modelde olmayan yabancılar 58-74 aralığında çıkabildiği için bazıları yanlışlıkla tanınabilir (README).
    /// </summary>
    public const double DefaultThreshold = 70;

    /// <summary>Bir etiketin eğitime alınması için gereken en az örnek sayısı.</summary>
    public const int MinSamplesPerLabel = 5;

    private readonly string _datasetDirectory;
    private readonly string _modelPath;
    private readonly string _labelsPath;
    private readonly object _lock = new(); // Kamera iş parçacığı tahmin yaparken eğitim modeli değiştirebilir.
    private LBPHFaceRecognizer? _model;
    private Dictionary<int, string> _labels = [];

    public FaceRecognizerService(string baseDirectory, string datasetDirectory)
    {
        _datasetDirectory = datasetDirectory;
        _modelPath = Path.Combine(baseDirectory, "model.yml");
        _labelsPath = Path.Combine(baseDirectory, "labels.json");
    }

    /// <summary>Tanıma eşiği (mesafe). Düşük eşik daha seçici olur.</summary>
    public double Threshold { get; set; } = DefaultThreshold;

    /// <summary>Eğitilmiş ya da yüklenmiş bir model var mı?</summary>
    public bool IsTrained
    {
        get { lock (_lock) return _model != null; }
    }

    /// <summary>Modeldeki etiket adları.</summary>
    public IReadOnlyCollection<string> Labels
    {
        get { lock (_lock) return _labels.Values.ToList(); }
    }

    /// <summary>Kayıtlı modeli yükler. Dosyalar yoksa false döner, bozuksa hata fırlatır.</summary>
    public bool TryLoad()
    {
        if (!File.Exists(_modelPath) || !File.Exists(_labelsPath)) return false;

        var labels = JsonSerializer.Deserialize<Dictionary<int, string>>(File.ReadAllText(_labelsPath))
            ?? throw new InvalidDataException("labels.json okunamadı.");
        var model = LBPHFaceRecognizer.Create();
        try
        {
            model.Read(_modelPath);
        }
        catch
        {
            model.Dispose();
            throw;
        }
        ReplaceModel(model, labels);
        return true;
    }

    /// <summary>
    /// dataset/&lt;etiket&gt;/*.png dosyalarıyla modeli eğitip kaydeder.
    /// Etiketler dengelenir: her etiketten en az örneği olan etiket kadar örnek rastgele seçilir,
    /// böylece çok örnekli etiket yabancı yüzleri kendine çekmez. MinSamplesPerLabel altındaki etiketler atlanır.
    /// </summary>
    public (int SamplesPerLabel, List<string> Labels, List<string> SkippedLabels) Train()
    {
        if (!Directory.Exists(_datasetDirectory))
            throw new InvalidOperationException("Henüz hiç örnek eklenmedi. Önce 'Veri Ekle' bölümünü kullanın.");

        var filesByLabel = Directory.GetDirectories(_datasetDirectory).Order()
            .Select(directory => (Label: Path.GetFileName(directory), Files: Directory.GetFiles(directory, "*.png")))
            .Where(x => x.Files.Length > 0)
            .ToList();
        var skipped = filesByLabel.Where(x => x.Files.Length < MinSamplesPerLabel)
            .Select(x => $"{x.Label} ({x.Files.Length} örnek)").ToList();
        filesByLabel.RemoveAll(x => x.Files.Length < MinSamplesPerLabel);

        if (filesByLabel.Count < 2)
            throw new InvalidOperationException(
                $"Eğitim için en az {MinSamplesPerLabel} örneği olan 2 farklı etiket gerekli (şu an {filesByLabel.Count} etiket var).");

        int perLabel = filesByLabel.Min(x => x.Files.Length);
        var random = new Random(0); // Sabit tohum: aynı veriyle her eğitim aynı örnekleri seçer.

        var images = new List<Mat>();
        var ids = new List<int>();
        var labels = new Dictionary<int, string>();
        try
        {
            foreach (var (label, files) in filesByLabel)
            {
                int id = labels.Count;
                labels[id] = label;
                foreach (string file in files.OrderBy(_ => random.Next()).Take(perLabel))
                {
                    var image = Cv2.ImDecode(File.ReadAllBytes(file), ImreadModes.Grayscale);
                    if (image.Empty())
                    {
                        image.Dispose();
                        continue;
                    }
                    if (image.Width != FaceDetector.FaceSize || image.Height != FaceDetector.FaceSize)
                        Cv2.Resize(image, image, new OpenCvSharp.Size(FaceDetector.FaceSize, FaceDetector.FaceSize));
                    images.Add(image);
                    ids.Add(id);
                }
            }

            var model = LBPHFaceRecognizer.Create();
            try
            {
                model.Train(images, ids);
                model.Write(_modelPath);
                File.WriteAllText(_labelsPath, JsonSerializer.Serialize(labels, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch
            {
                model.Dispose();
                throw;
            }
            ReplaceModel(model, labels);
            return (perLabel, labels.Values.ToList(), skipped);
        }
        finally
        {
            foreach (var image in images) image.Dispose();
        }
    }

    /// <summary>200x200 gri yüzü tanır. Model yoksa Confidence NaN olur.</summary>
    public RecognitionResult Predict(Mat face, Rect location)
    {
        lock (_lock)
        {
            if (_model == null) return new RecognitionResult(location, "", double.NaN, false);

            _model.Predict(face, out int id, out double distance);
            string label = _labels.GetValueOrDefault(id, "?");
            return new RecognitionResult(location, label, distance, distance <= Threshold);
        }
    }

    /// <summary>Gri resimdeki her yüzü kırpıp tanır.</summary>
    public List<RecognitionResult> RecognizeAll(Mat gray, IEnumerable<Rect> faces)
    {
        var results = new List<RecognitionResult>();
        foreach (var face in faces)
        {
            using var crop = FaceDetector.CropFace(gray, face);
            results.Add(Predict(crop, face));
        }
        return results;
    }

    public void Dispose() => ReplaceModel(null, []);

    private void ReplaceModel(LBPHFaceRecognizer? model, Dictionary<int, string> labels)
    {
        lock (_lock)
        {
            _model?.Dispose();
            _model = model;
            _labels = labels;
        }
    }
}
