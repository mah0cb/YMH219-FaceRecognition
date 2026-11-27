using OpenCvSharp;

namespace FaceRecognitionApp.Services;

/// <summary>Yüz dikdörtgenlerini ve etiketlerini Bitmap üzerine çizer. GDI+ kullanıldığı için Türkçe karakterler doğru yazılır.</summary>
public static class FaceOverlay
{
    public static void Draw(Bitmap bitmap, IEnumerable<(Rect Face, string Text, Color Color)> items)
    {
        using var graphics = Graphics.FromImage(bitmap);
        float scale = Math.Max(1f, bitmap.Width / 640f);
        using var font = new Font("Segoe UI", 14 * scale, FontStyle.Bold, GraphicsUnit.Pixel);

        foreach (var (face, text, color) in items)
        {
            using var pen = new Pen(color, 2 * scale);
            graphics.DrawRectangle(pen, face.X, face.Y, face.Width, face.Height);

            var textSize = graphics.MeasureString(text, font);
            float textY = Math.Max(0, face.Y - textSize.Height);
            using var background = new SolidBrush(color);
            graphics.FillRectangle(background, face.X, textY, textSize.Width, textSize.Height);
            graphics.DrawString(text, font, Brushes.Black, face.X, textY);
        }
    }
}
