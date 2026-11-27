using OpenCvSharp;

namespace FaceRecognitionApp.Services;

/// <summary>Yüz örneklerini dataset/&lt;etiket&gt;/ klasörüne, algılanan yüzleri detected_faces/ klasörüne kaydeder.</summary>
public sealed class DatasetManager
{
    private readonly string _datasetDirectory;
    private readonly string _detectedFacesDirectory;
    private int _fileCounter;

    public DatasetManager(string baseDirectory)
    {
        _datasetDirectory = Path.Combine(baseDirectory, "dataset");
        _detectedFacesDirectory = Path.Combine(baseDirectory, "detected_faces");
    }

    /// <summary>Eğitim örneklerinin bulunduğu klasör (her etiket bir alt klasör).</summary>
    public string DatasetDirectory => _datasetDirectory;

    /// <summary>Etiket adını kırpar ve klasör adı olarak geçerli mi kontrol eder. Geçersizse ArgumentException fırlatır.</summary>
    public static string NormalizeLabel(string label)
    {
        label = label.Trim();
        if (label.Length == 0)
            throw new ArgumentException("Etiket adı boş olamaz.");
        if (label is "." or ".." || label.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("Etiket adında \\ / : * ? \" < > | karakterleri kullanılamaz.");
        return label;
    }

    /// <summary>Etiketteki kayıtlı örnek sayısı.</summary>
    public int CountSamples(string label)
    {
        string directory = Path.Combine(_datasetDirectory, NormalizeLabel(label));
        return Directory.Exists(directory) ? Directory.GetFiles(directory, "*.png").Length : 0;
    }

    /// <summary>Gri resimdeki yüzü 200x200 gri olarak etiket klasörüne kaydeder.</summary>
    public void SaveSample(string label, Mat gray, Rect face)
    {
        string directory = Path.Combine(_datasetDirectory, NormalizeLabel(label));
        Directory.CreateDirectory(directory);
        using var crop = FaceDetector.CropFace(gray, face);
        WritePng(crop, Path.Combine(directory, NewFileName()));
    }

    /// <summary>
    /// Seçilen her fotoğraftaki ana yüzü (gözü görünen en büyük yüz) kırpıp etikete ekler; arka plandaki kişiler ve yanlış algılamalar eklenmez.
    /// Eklenen yüz sayısını ve sorunlu dosyaları döndürür.
    /// </summary>
    public (int FacesAdded, List<string> Problems) AddFromFiles(IEnumerable<string> paths, string label, FaceDetector detector)
    {
        label = NormalizeLabel(label);
        int added = 0;
        var problems = new List<string>();
        foreach (string path in paths)
        {
            string name = Path.GetFileName(path);
            try
            {
                using var image = ImageLoader.Load(path);
                using var gray = FaceDetector.ToGray(image);
                var faces = detector.Detect(gray);
                if (faces.Length == 0)
                {
                    problems.Add($"{name}: yüz bulunamadı");
                    continue;
                }
                if (faces.Length > 1) problems.Add($"{name}: {faces.Length} yüz bulundu, sadece ana yüz eklendi");
                SaveSample(label, gray, detector.SelectMainFace(gray, faces));
                added++;
            }
            catch (Exception ex)
            {
                problems.Add($"{name}: {ex.Message}");
            }
        }
        return (added, problems);
    }

    /// <summary>Resimdeki yüzleri renkli olarak detected_faces/ klasörüne kaydeder ve klasör yolunu döndürür.</summary>
    public string SaveDetectedFaces(Mat bgr, IEnumerable<Rect> faces)
    {
        Directory.CreateDirectory(_detectedFacesDirectory);
        foreach (var face in faces)
        {
            using var crop = new Mat(bgr, face);
            WritePng(crop, Path.Combine(_detectedFacesDirectory, NewFileName()));
        }
        return _detectedFacesDirectory;
    }

    private string NewFileName() =>
        $"{DateTime.Now:yyyyMMdd_HHmmss_fff}_{Interlocked.Increment(ref _fileCounter)}.png";

    /// <summary>Türkçe karakterli yollarda da çalışsın diye PNG'yi bellekte kodlayıp File ile yazar.</summary>
    private static void WritePng(Mat image, string path)
    {
        Cv2.ImEncode(".png", image, out byte[] bytes);
        File.WriteAllBytes(path, bytes);
    }
}
