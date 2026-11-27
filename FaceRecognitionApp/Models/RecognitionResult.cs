using OpenCvSharp;

namespace FaceRecognitionApp.Models;

/// <summary>Bir yüzün tanıma sonucu. Confidence LBPH mesafesidir: düşük değer daha iyi eşleşme demektir.</summary>
/// <param name="Face">Yüzün resimdeki konumu.</param>
/// <param name="Label">Modelin en yakın bulduğu etiket.</param>
/// <param name="Confidence">LBPH mesafesi; model yoksa NaN.</param>
/// <param name="IsKnown">Mesafe eşiğin altında mı (tanındı mı)?</param>
public sealed record RecognitionResult(Rect Face, string Label, double Confidence, bool IsKnown)
{
    /// <summary>Model eğitilmemişken sadece algılanan yüz için kullanılır.</summary>
    public bool HasPrediction => !double.IsNaN(Confidence);

    /// <summary>Yüzün üstüne yazılacak kısa metin, örn. "Mahmut (42,3)" veya "Bilinmiyor (95,1)".</summary>
    public string DisplayText => HasPrediction ? $"{(IsKnown ? Label : "Bilinmiyor")} ({Confidence:F1})" : "Yüz";

    /// <summary>Sonuç panelinde gösterilecek açıklamalı metin.</summary>
    public string Summary => !HasPrediction
        ? "Yüz algılandı (model eğitilmedi)"
        : IsKnown
            ? $"{Label} - güven skoru (mesafe): {Confidence:F1}"
            : $"Bilinmiyor - en yakın: {Label}, mesafe: {Confidence:F1}";
}
