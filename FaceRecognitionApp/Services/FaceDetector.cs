using OpenCvSharp;

namespace FaceRecognitionApp.Services;

/// <summary>Haar cascade ile resimdeki yüzleri bulur; örtüşen adaylar arasından göz cascade'iyle doğru olanı seçer.</summary>
public sealed class FaceDetector : IDisposable
{
    /// <summary>Kaydedilen ve tanınan yüzlerin kenar uzunluğu (piksel).</summary>
    public const int FaceSize = 200;

    /// <summary>Bir adayın yüz sayılması için gereken komşu tespit sayısı. Daha yüksek değerler fotoğraflardaki gerçek yüzleri kaçırıyor.</summary>
    public const int MinNeighbors = 5;

    /// <summary>Cascade dosyalarının beklendiği klasör (exe'nin yanındaki Models).</summary>
    public static string DefaultModelsDirectory => Path.Combine(AppContext.BaseDirectory, "Models");

    private readonly CascadeClassifier _faceCascade;
    private readonly CascadeClassifier _eyeCascade;
    private readonly object _lock = new(); // Kamera ve arayüz iş parçacıkları aynı anda kullanabilir.

    /// <summary>Yüz ve göz cascade dosyalarını yükler. Dosya yoksa FileNotFoundException, bozuksa InvalidDataException fırlatır.</summary>
    public FaceDetector(string modelsDirectory)
    {
        _faceCascade = LoadCascade(Path.Combine(modelsDirectory, "haarcascade_frontalface_default.xml"));
        try
        {
            _eyeCascade = LoadCascade(Path.Combine(modelsDirectory, "haarcascade_eye.xml"));
        }
        catch
        {
            _faceCascade.Dispose();
            throw;
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
            var candidates = _faceCascade.DetectMultiScale(equalized, scaleFactor: 1.1, minNeighbors: MinNeighbors,
                HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(minSide, minSide));
            return candidates.Length < 2 ? candidates : RemoveOverlapping(equalized, candidates);
        }
    }

    /// <summary>
    /// Fotoğraftaki ana yüzü seçer: gözü görünen adaylar arasından en büyüğü, hiçbirinde göz yoksa en büyük aday.
    /// Yalnızca boyuta bakmak arka plandaki büyük bir yanlış algılamayı (ör. duvar deseni) seçebiliyor.
    /// </summary>
    public Rect SelectMainFace(Mat gray, Rect[] faces)
    {
        if (faces.Length == 1) return faces[0];
        using var equalized = new Mat();
        Cv2.EqualizeHist(gray, equalized);
        lock (_lock)
        {
            return faces
                .OrderByDescending(face => CountEyes(equalized, face) > 0)
                .ThenByDescending(face => face.Width * face.Height)
                .First();
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

    public void Dispose()
    {
        _faceCascade.Dispose();
        _eyeCascade.Dispose();
    }

    /// <summary>
    /// Birbirine değen adaylardan gözleri daha net görüneni tutar (eşitse büyük olanı).
    /// Gerçek yüzün yanındaki ayna/çekmece parçası ya da yüzü içine alan büyük yanlış kutu böyle elenir.
    /// </summary>
    private Rect[] RemoveOverlapping(Mat equalized, Rect[] candidates)
    {
        var ranked = candidates
            .OrderByDescending(face => CountEyes(equalized, face))
            .ThenByDescending(face => face.Width);
        var kept = new List<Rect>();
        foreach (var face in ranked)
            if (!kept.Any(k => k.IntersectsWith(face))) kept.Add(face);
        return kept.ToArray();
    }

    /// <summary>Aday kutunun üst kısmında bulunan göz sayısı (en fazla 2).</summary>
    private int CountEyes(Mat equalized, Rect face)
    {
        using var region = new Mat(equalized, face);
        using var resized = new Mat();
        Cv2.Resize(region, resized, new OpenCvSharp.Size(FaceSize, FaceSize));
        using var upperPart = new Mat(resized, new Rect(0, 0, FaceSize, FaceSize * 3 / 5));
        var eyes = _eyeCascade.DetectMultiScale(upperPart, 1.1, 3, 0, new OpenCvSharp.Size(20, 20), new OpenCvSharp.Size(90, 90));
        return Math.Min(2, eyes.Length);
    }

    private static CascadeClassifier LoadCascade(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Algılama modeli (cascade XML) bulunamadı:\n" + path, path);

        var cascade = new CascadeClassifier(path);
        if (cascade.Empty())
        {
            cascade.Dispose();
            throw new InvalidDataException("Cascade dosyası okunamadı veya bozuk:\n" + path);
        }
        return cascade;
    }
}
