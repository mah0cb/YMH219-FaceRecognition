using OpenCvSharp;

namespace FaceRecognitionApp.Services;

/// <summary>Haar cascade ile resimdeki yüzleri bulur.</summary>
public sealed class FaceDetector : IDisposable
{
    /// <summary>Kaydedilen ve tanınan yüzlerin kenar uzunluğu (piksel).</summary>
    public const int FaceSize = 200;

    /// <summary>Exe'nin yanındaki Models klasöründe beklenen cascade dosyası.</summary>
    public static string DefaultCascadePath =>
        Path.Combine(AppContext.BaseDirectory, "Models", "haarcascade_frontalface_default.xml");

    private readonly CascadeClassifier _cascade;
    private readonly object _lock = new(); // Kamera ve arayüz iş parçacıkları aynı anda kullanabilir.

    /// <summary>Cascade dosyasını yükler. Dosya yoksa FileNotFoundException, bozuksa InvalidDataException fırlatır.</summary>
    public FaceDetector(string cascadePath)
    {
        if (!File.Exists(cascadePath))
            throw new FileNotFoundException("Yüz algılama modeli (cascade XML) bulunamadı:\n" + cascadePath, cascadePath);

        _cascade = new CascadeClassifier(cascadePath);
        if (_cascade.Empty())
        {
            _cascade.Dispose();
            throw new InvalidDataException("Cascade dosyası okunamadı veya bozuk:\n" + cascadePath);
        }
    }

    /// <summary>BGR resmi gri tona çevirir.</summary>
    public static Mat ToGray(Mat bgr)
    {
        var gray = new Mat();
        Cv2.CvtColor(bgr, gray, ColorConversionCodes.BGR2GRAY);
        return gray;
    }

    /// <summary>Gri resimdeki yüzlerin dikdörtgenlerini döndürür. Önce histogram eşitlenir (ışık farkını azaltır).</summary>
    public Rect[] Detect(Mat gray)
    {
        using var equalized = new Mat();
        Cv2.EqualizeHist(gray, equalized);
        int minSide = Math.Max(40, Math.Min(gray.Width, gray.Height) / 15);
        lock (_lock)
        {
            return _cascade.DetectMultiScale(equalized, scaleFactor: 1.1, minNeighbors: 5,
                HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(minSide, minSide));
        }
    }

    /// <summary>Yüzü kırpar, 200x200 boyutuna getirir ve histogramını eşitler. Eğitim ve tanıma aynı işlemi kullanır.</summary>
    public static Mat CropFace(Mat gray, Rect face)
    {
        using var region = new Mat(gray, face);
        var result = new Mat();
        Cv2.Resize(region, result, new OpenCvSharp.Size(FaceSize, FaceSize), 0, 0, InterpolationFlags.Area);
        Cv2.EqualizeHist(result, result);
        return result;
    }

    public void Dispose() => _cascade.Dispose();
}
