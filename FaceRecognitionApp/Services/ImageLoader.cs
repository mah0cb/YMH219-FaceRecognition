using OpenCvSharp;

namespace FaceRecognitionApp.Services;

/// <summary>Resim dosyalarını okur. Türkçe karakterli yollarda da çalışsın diye dosya byte olarak okunup çözülür.</summary>
public static class ImageLoader
{
    /// <summary>Büyük telefon fotoğrafları bu uzun kenara küçültülür (algılama hızlansın diye).</summary>
    private const int MaxSide = 1600;

    /// <summary>Resmi BGR Mat olarak yükler. Okunamazsa InvalidDataException fırlatır.</summary>
    public static Mat Load(string path)
    {
        var image = Cv2.ImDecode(File.ReadAllBytes(path), ImreadModes.Color);
        if (image.Empty())
        {
            image.Dispose();
            throw new InvalidDataException($"'{Path.GetFileName(path)}' resim olarak okunamadı (desteklenen: JPG, PNG, BMP).");
        }

        int longSide = Math.Max(image.Width, image.Height);
        if (longSide > MaxSide)
        {
            double scale = (double)MaxSide / longSide;
            Cv2.Resize(image, image, new OpenCvSharp.Size(), scale, scale, InterpolationFlags.Area);
        }
        return image;
    }
}
