using FaceRecognitionApp.Services;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace FaceRecognitionApp;

/// <summary>Uygulamanın ana penceresi. Sadece arayüz ve olay bağlama işini yapar.</summary>
public partial class MainForm : Form
{
    private readonly CameraService _camera = new();
    private FaceDetector? _detector;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        cmbKamera.SelectedIndex = 0;
        _camera.FrameReady += Camera_FrameReady;
        try
        {
            _detector = new FaceDetector(FaceDetector.DefaultCascadePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + "\n\nYüz algılama ve tanıma devre dışı; sadece görüntü gösterilecek.",
                "Model Dosyası Bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        UpdateButtons();
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        _camera.Dispose();
        _detector?.Dispose();
    }

    private async void btnKameraBaslat_Click(object sender, EventArgs e)
    {
        // ComboBox'ta 0. öğe "Otomatik" (-1), diğerleri 0-3 indeksleri.
        int requestedIndex = cmbKamera.SelectedIndex - 1;
        btnKameraBaslat.Enabled = false;
        SetStatus("Kamera açılıyor...");
        try
        {
            int index = await Task.Run(() => _camera.Start(requestedIndex));
            SetStatus($"Kamera {index} çalışıyor.");
        }
        catch (Exception ex)
        {
            SetStatus("Kamera açılamadı.");
            MessageBox.Show("Kamera açılamadı.\n" + ex.Message +
                "\n\nIriun kullanıyorsanız telefon uygulamasının ve bilgisayardaki Iriun Webcam programının açık olduğundan emin olun.",
                "Kamera Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        UpdateButtons();
    }

    private void btnKameraDurdur_Click(object sender, EventArgs e)
    {
        _camera.Stop();
        SetStatus("Kamera durduruldu.");
        UpdateButtons();
    }

    private void btnGoruntuSec_Click(object sender, EventArgs e)
    {
        if (ofdGoruntu.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            _camera.Stop();
            UpdateButtons();
            using var image = ImageLoader.Load(ofdGoruntu.FileName);
            var (bitmap, faces) = ProcessFrame(image);
            ShowImage(bitmap);
            SetStatus($"{Path.GetFileName(ofdGoruntu.FileName)}: {faces.Length} yüz bulundu.");
        }
        catch (Exception ex)
        {
            SetStatus("Görüntü açılamadı.");
            MessageBox.Show("Görüntü açılamadı.\n" + ex.Message, "Dosya Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    /// <summary>Kamera iş parçacığında çalışır; kareyi işleyip arayüze gönderir.</summary>
    private void Camera_FrameReady(object? sender, Mat frame)
    {
        using (frame)
        {
            try
            {
                ShowImage(ProcessFrame(frame).Image);
            }
            catch (Exception ex)
            {
                BeginInvoke(() => SetStatus("Kare işlenemedi: " + ex.Message));
            }
        }
    }

    /// <summary>Karedeki yüzleri bulur ve dikdörtgenle işaretlenmiş Bitmap döndürür.</summary>
    private (Bitmap Image, OpenCvSharp.Rect[] Faces) ProcessFrame(Mat bgr)
    {
        var bitmap = bgr.ToBitmap();
        if (_detector == null) return (bitmap, []);

        using var gray = FaceDetector.ToGray(bgr);
        var faces = _detector.Detect(gray);
        FaceOverlay.Draw(bitmap, faces.Select(face => (face, "Yüz", Color.Lime)));
        return (bitmap, faces);
    }

    /// <summary>PictureBox'taki resmi değiştirir ve eski Bitmap'i Dispose eder. Her iş parçacığından çağrılabilir.</summary>
    private void ShowImage(Bitmap bitmap)
    {
        if (InvokeRequired)
        {
            try { BeginInvoke(() => ShowImage(bitmap)); }
            catch (InvalidOperationException) { bitmap.Dispose(); } // Form kapanıyor.
            return;
        }
        if (IsDisposed)
        {
            bitmap.Dispose();
            return;
        }
        var old = picGoruntu.Image;
        picGoruntu.Image = bitmap;
        old?.Dispose();
    }

    private void UpdateButtons()
    {
        btnKameraBaslat.Enabled = !_camera.IsRunning;
        btnKameraDurdur.Enabled = _camera.IsRunning;
        cmbKamera.Enabled = !_camera.IsRunning;
    }

    private void SetStatus(string message) => lblDurum.Text = message;
}
