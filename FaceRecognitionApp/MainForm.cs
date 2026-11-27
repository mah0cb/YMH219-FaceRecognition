using FaceRecognitionApp.Models;
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
    private readonly FaceRecognizerService _recognizer;
    private FaceDetector? _detector;

    private readonly object _imageLock = new();
    private Mat? _currentImage; // Ekrandaki son kare veya seçilen resim ("Yüzleri Kaydet" için).

    private int _samplesLeft;
    private string _sampleLabel = "";
    private long _lastSampleTime;

    public MainForm()
    {
        InitializeComponent();
        _recognizer = new FaceRecognizerService(AppContext.BaseDirectory, _dataset.DatasetDirectory);
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        cmbKamera.SelectedIndex = 0;
        nudEsik.Value = (decimal)FaceRecognizerService.DefaultThreshold;
        _camera.FrameReady += Camera_FrameReady;
        _camera.Error += Camera_Error;
        try
        {
            _detector = new FaceDetector(FaceDetector.DefaultModelsDirectory);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + "\n\nYüz algılama ve tanıma devre dışı; sadece görüntü gösterilecek.",
                "Model Dosyası Bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        LoadRecognitionModel();
        UpdateButtons();
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        _camera.Dispose();
        _detector?.Dispose();
        _recognizer.Dispose();
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
            var (bitmap, results) = ProcessFrame(image);
            SetCurrentImage(image);
            ShowFrame(bitmap, results);
            SetStatus($"{Path.GetFileName(ofdGoruntu.FileName)}: {results.Count} yüz bulundu.");
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

    private async void btnModeliEgit_Click(object sender, EventArgs e)
    {
        btnModeliEgit.Enabled = false;
        SetStatus("Model eğitiliyor...");
        try
        {
            var (perLabel, labels, skipped) = await Task.Run(_recognizer.Train);
            SetStatus($"Model eğitildi: {string.Join(", ", labels)}; her etiketten {perLabel} örnek (dengeli).");
            if (skipped.Count > 0)
                MessageBox.Show($"Şu etiketlerde {FaceRecognizerService.MinSamplesPerLabel}'ten az örnek olduğu için eğitime alınmadı:\n" +
                    string.Join("\n", skipped), "Eğitim", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshStillImage();
        }
        catch (Exception ex)
        {
            SetStatus("Model eğitilemedi.");
            MessageBox.Show("Model eğitilemedi.\n" + ex.Message, "Eğitim Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        btnModeliEgit.Enabled = true;
    }

    private void nudEsik_ValueChanged(object sender, EventArgs e)
    {
        _recognizer.Threshold = (double)nudEsik.Value;
        RefreshStillImage();
    }

    /// <summary>Program açılışında kayıtlı modeli yükler.</summary>
    private void LoadRecognitionModel()
    {
        try
        {
            SetStatus(_recognizer.TryLoad()
                ? $"Model yüklendi: {string.Join(", ", _recognizer.Labels)}."
                : "Eğitilmiş model yok. Veri ekleyip 'Modeli Eğit' düğmesine basın.");
        }
        catch (Exception ex)
        {
            SetStatus("Model yüklenemedi.");
            MessageBox.Show("Kayıtlı model (model.yml / labels.json) okunamadı. Modeli yeniden eğitin.\n" + ex.Message,
                "Model Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    /// <summary>Kamera kapalıyken ekrandaki resmi yeni model/eşikle yeniden işler.</summary>
    private void RefreshStillImage()
    {
        if (_camera.IsRunning) return;
        Mat? snapshot;
        lock (_imageLock) snapshot = _currentImage?.Clone();
        if (snapshot == null) return;
        using (snapshot)
        {
            var (bitmap, results) = ProcessFrame(snapshot);
            ShowFrame(bitmap, results);
        }
    }

    /// <summary>Kamera koptuğunda kamera iş parçacığından gelir; kamerayı durdurup kullanıcıyı uyarır.</summary>
    private void Camera_Error(object? sender, string message)
    {
        try
        {
            BeginInvoke(() =>
            {
                StopCamera();
                SetStatus(message);
                MessageBox.Show(message, "Kamera Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            });
        }
        catch (InvalidOperationException)
        {
            // Form kapanıyor.
        }
    }

    /// <summary>Kamera iş parçacığında çalışır; kareyi işleyip arayüze gönderir.</summary>
    private void Camera_FrameReady(object? sender, Mat frame)
    {
        try
        {
            var (bitmap, results) = ProcessFrame(frame);
            TryTakeSample(frame, results);
            SetCurrentImage(frame);
            ShowFrame(bitmap, results);
        }
        catch (Exception ex)
        {
            frame.Dispose();
            BeginInvoke(() => SetStatus("Kare işlenemedi: " + ex.Message));
        }
    }

    /// <summary>Karedeki yüzleri bulup tanır; her yüzü dikdörtgen ve etiketle işaretlenmiş Bitmap döndürür.</summary>
    private (Bitmap Image, List<RecognitionResult> Results) ProcessFrame(Mat bgr)
    {
        var bitmap = bgr.ToBitmap();
        if (_detector == null) return (bitmap, []);

        using var gray = FaceDetector.ToGray(bgr);
        var results = _recognizer.RecognizeAll(gray, _detector.Detect(gray));
        FaceOverlay.Draw(bitmap, results.Select(r => (r.Face, r.DisplayText,
            !r.HasPrediction ? Color.Yellow : r.IsKnown ? Color.Lime : Color.OrangeRed)));
        return (bitmap, results);
    }

    /// <summary>Örnek alma sürerken karede tek yüz varsa belirli aralıklarla bir örnek kaydeder.</summary>
    private void TryTakeSample(Mat frame, List<RecognitionResult> faces)
    {
        if (_samplesLeft <= 0 || Environment.TickCount64 - _lastSampleTime < SampleIntervalMs) return;
        if (faces.Count != 1)
        {
            BeginInvoke(() => SetStatus("Örnek almak için kadrajda tek bir yüz olmalı."));
            return;
        }

        try
        {
            using var gray = FaceDetector.ToGray(frame);
            _dataset.SaveSample(_sampleLabel, gray, faces[0].Face);
        }
        catch (Exception ex)
        {
            _samplesLeft = 0;
            BeginInvoke(() =>
            {
                UpdateButtons();
                MessageBox.Show("Örnek kaydedilemedi, örnek alma durduruldu.\n" + ex.Message,
                    "Dosya Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            });
            return;
        }
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

    /// <summary>Resmi ve tanıma sonuçlarını gösterir, eski Bitmap'i Dispose eder. Her iş parçacığından çağrılabilir.</summary>
    private void ShowFrame(Bitmap bitmap, List<RecognitionResult> results)
    {
        if (InvokeRequired)
        {
            try { BeginInvoke(() => ShowFrame(bitmap, results)); }
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

        lblSonuc.Text = results.Count == 0
            ? "Tahmin: yüz bulunamadı"
            : string.Join("\n", results.Select((r, i) => $"{i + 1}. {r.Summary}"));
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
