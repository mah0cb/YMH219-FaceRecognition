using System.Text.Json;
using FaceRecognitionApp.Models;
using OpenCvSharp;
using OpenCvSharp.Face;

namespace FaceRecognitionApp.Services;

/// <summary>LBPH algoritmasıyla yüz tanır. dataset/ klasöründen eğitir, modeli model.yml, etiketleri labels.json olarak saklar.</summary>
public sealed class FaceRecognizerService : IDisposable
{
    /// <summary>Varsayılan eşik. LBPH mesafesi bunun üstündeyse yüz "Bilinmiyor" sayılır.</summary>
    public const double DefaultThreshold = 80;

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

    /// <summary>dataset/&lt;etiket&gt;/*.png dosyalarıyla modeli eğitip kaydeder. Örnek ve etiket sayısını döndürür.</summary>
    public (int Samples, int Labels) Train()
    {
        if (!Directory.Exists(_datasetDirectory))
            throw new InvalidOperationException("Henüz hiç örnek eklenmedi. Önce 'Veri Ekle' bölümünü kullanın.");

        var images = new List<Mat>();
        var ids = new List<int>();
        var labels = new Dictionary<int, string>();
        try
        {
            foreach (string directory in Directory.GetDirectories(_datasetDirectory).Order())
            {
                string[] files = Directory.GetFiles(directory, "*.png");
                if (files.Length == 0) continue;

                int id = labels.Count;
                labels[id] = Path.GetFileName(directory);
                foreach (string file in files)
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

            if (labels.Count < 2)
                throw new InvalidOperationException($"Eğitim için en az 2 farklı etiket gerekli (şu an {labels.Count} etiket var).");

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
            return (images.Count, labels.Count);
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
