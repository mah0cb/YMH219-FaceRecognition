using FaceRecognitionApp.Services;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace FaceRecognitionApp;

/// <summary>Uygulamanın ana penceresi. Sadece arayüz ve olay bağlama işini yapar.</summary>
public partial class MainForm : Form
{
    private readonly CameraService _camera = new();

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        cmbKamera.SelectedIndex = 0;
        _camera.FrameReady += Camera_FrameReady;
        UpdateButtons();
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        _camera.Dispose();
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

    /// <summary>Kamera iş parçacığında çalışır; kareyi Bitmap'e çevirip arayüze gönderir.</summary>
    private void Camera_FrameReady(object? sender, Mat frame)
    {
        using (frame)
        {
            ShowImage(frame.ToBitmap());
        }
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
