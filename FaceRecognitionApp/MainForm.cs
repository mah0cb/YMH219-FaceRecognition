using FaceRecognitionApp.Services;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace FaceRecognitionApp;

/// <summary>Uygulamanın ana penceresi. Sadece arayüz ve olay bağlama işini yapar.</summary>
public partial class MainForm : Form
{
    /// <summary>"Kameradan Örnek Al" ile kaydedilecek yüz sayısı.</summary>
    private const int SamplesPerCapture = 20;

    /// <summary>İki örnek arasındaki en kısa süre (ms); ardışık karelerin birbirinin aynısı olmasını önler.</summary>
    private const int SampleIntervalMs = 300;

    private readonly CameraService _camera = new();
    private readonly DatasetManager _dataset = new(AppContext.BaseDirectory);
    private FaceDetector? _detector;

    private readonly object _imageLock = new();
    private Mat? _currentImage; // Ekrandaki son kare veya seçilen resim ("Yüzleri Kaydet" için).

    private int _samplesLeft;
    private string _sampleLabel = "";
    private long _lastSampleTime;

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
        SetCurrentImage(null);
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
        StopCamera();
        SetStatus("Kamera durduruldu.");
    }

    private void btnGoruntuSec_Click(object sender, EventArgs e)
    {
        if (ofdGoruntu.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            StopCamera();
            var image = ImageLoader.Load(ofdGoruntu.FileName);
            var (bitmap, faces) = ProcessFrame(image);
            SetCurrentImage(image);
            ShowImage(bitmap);
            SetStatus($"{Path.GetFileName(ofdGoruntu.FileName)}: {faces.Length} yüz bulundu.");
        }
        catch (Exception ex)
        {
            SetStatus("Görüntü açılamadı.");
            MessageBox.Show("Görüntü açılamadı.\n" + ex.Message, "Dosya Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void btnYuzleriKaydet_Click(object sender, EventArgs e)
    {
        if (_detector == null) return;
        Mat? snapshot;
        lock (_imageLock) snapshot = _currentImage?.Clone();
        if (snapshot == null)
        {
            MessageBox.Show("Önce kamerayı başlatın veya bir görüntü seçin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            using (snapshot)
            using (var gray = FaceDetector.ToGray(snapshot))
            {
                var faces = _detector.Detect(gray);
                if (faces.Length == 0)
                {
                    SetStatus("Kaydedilecek yüz bulunamadı.");
                    return;
                }
                string folder = _dataset.SaveDetectedFaces(snapshot, faces);
                SetStatus($"{faces.Length} yüz kaydedildi: {folder}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Yüzler kaydedilemedi.\n" + ex.Message, "Dosya Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void txtEtiket_TextChanged(object sender, EventArgs e) => UpdateSampleCount();

    private void btnKameradanOrnekAl_Click(object sender, EventArgs e)
    {
        if (!TryGetLabel(out string label)) return;
        if (!_camera.IsRunning)
        {
            MessageBox.Show("Önce kamerayı başlatın.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        _sampleLabel = label;
        _samplesLeft = SamplesPerCapture;
        SetStatus($"'{label}' için örnek alınıyor; kadrajda sadece siz olun, başınızı hafifçe çevirin.");
        UpdateButtons();
    }

    private async void btnDosyalardanEkle_Click(object sender, EventArgs e)
    {
        if (_detector == null || !TryGetLabel(out string label)) return;
        if (ofdOrnekler.ShowDialog(this) != DialogResult.OK) return;

        var detector = _detector;
        string[] files = ofdOrnekler.FileNames;
        btnDosyalardanEkle.Enabled = false;
        SetStatus($"{files.Length} dosya işleniyor...");
        try
        {
            var (added, problems) = await Task.Run(() => _dataset.AddFromFiles(files, label, detector));
            SetStatus($"'{label}' etiketine {added} yüz eklendi.");
            string message = $"{files.Length} dosyadan {added} yüz '{label}' etiketine eklendi.";
            if (problems.Count > 0) message += "\n\nUyarılar:\n" + string.Join("\n", problems.Take(15));
            MessageBox.Show(message, "Dosyalardan Ekle", MessageBoxButtons.OK,
                added > 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Dosyalar eklenemedi.\n" + ex.Message, "Dosya Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        UpdateSampleCount();
        UpdateButtons();
    }

    /// <summary>Kamera iş parçacığında çalışır; kareyi işleyip arayüze gönderir.</summary>
    private void Camera_FrameReady(object? sender, Mat frame)
    {
        try
        {
            var (bitmap, faces) = ProcessFrame(frame);
            TryTakeSample(frame, faces);
            SetCurrentImage(frame);
            ShowImage(bitmap);
        }
        catch (Exception ex)
        {
            frame.Dispose();
            BeginInvoke(() => SetStatus("Kare işlenemedi: " + ex.Message));
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

    /// <summary>Örnek alma sürerken karede tek yüz varsa belirli aralıklarla bir örnek kaydeder.</summary>
    private void TryTakeSample(Mat frame, OpenCvSharp.Rect[] faces)
    {
        if (_samplesLeft <= 0 || Environment.TickCount64 - _lastSampleTime < SampleIntervalMs) return;
        if (faces.Length != 1)
        {
            BeginInvoke(() => SetStatus("Örnek almak için kadrajda tek bir yüz olmalı."));
            return;
        }

        using (var gray = FaceDetector.ToGray(frame))
            _dataset.SaveSample(_sampleLabel, gray, faces[0]);
        _lastSampleTime = Environment.TickCount64;
        int left = --_samplesLeft;

        BeginInvoke(() =>
        {
            SetStatus(left > 0
                ? $"Örnek alınıyor: {SamplesPerCapture - left}/{SamplesPerCapture}"
                : $"'{_sampleLabel}' için {SamplesPerCapture} örnek kaydedildi.");
            UpdateSampleCount();
            UpdateButtons();
        });
    }

    /// <summary>Etiket kutusundaki adı doğrular; geçersizse kullanıcıya bildirir.</summary>
    private bool TryGetLabel(out string label)
    {
        try
        {
            label = DatasetManager.NormalizeLabel(txtEtiket.Text);
            return true;
        }
        catch (ArgumentException ex)
        {
            label = "";
            MessageBox.Show(ex.Message, "Etiket", MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtEtiket.Focus();
            return false;
        }
    }

    private void UpdateSampleCount()
    {
        try
        {
            string label = DatasetManager.NormalizeLabel(txtEtiket.Text);
            lblOrnekSayisi.Text = $"'{label}' etiketinde {_dataset.CountSamples(label)} örnek var.";
        }
        catch (ArgumentException)
        {
            lblOrnekSayisi.Text = "Etiket adı girin.";
        }
    }

    /// <summary>Ekrandaki son resmi değiştirir ve eskisini Dispose eder. Mat'in sahipliği forma geçer.</summary>
    private void SetCurrentImage(Mat? image)
    {
        lock (_imageLock)
        {
            _currentImage?.Dispose();
            _currentImage = image;
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

    private void StopCamera()
    {
        _camera.Stop();
        _samplesLeft = 0;
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        bool running = _camera.IsRunning;
        bool hasDetector = _detector != null;
        btnKameraBaslat.Enabled = !running;
        btnKameraDurdur.Enabled = running;
        cmbKamera.Enabled = !running;
        btnKameradanOrnekAl.Enabled = running && hasDetector && _samplesLeft <= 0;
        btnDosyalardanEkle.Enabled = hasDetector;
        btnYuzleriKaydet.Enabled = hasDetector;
    }

    private void SetStatus(string message) => lblDurum.Text = message;
}
