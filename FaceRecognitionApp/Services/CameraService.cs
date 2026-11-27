using OpenCvSharp;

namespace FaceRecognitionApp.Services;

/// <summary>Kameradan arka planda kare okur ve her yeni karede FrameReady olayını tetikler.</summary>
public sealed class CameraService : IDisposable
{
    /// <summary>Otomatik aramada denenecek en büyük kamera indeksi.</summary>
    public const int MaxCameraIndex = 3;

    /// <summary>Üst üste bu kadar kare okunamazsa kamera kopmuş sayılır (yaklaşık 2 saniye).</summary>
    private const int MaxReadFailures = 60;

    private VideoCapture? _capture;
    private CancellationTokenSource? _cts;
    private Task? _loop;

    /// <summary>Yeni kare geldiğinde arka plan iş parçacığında tetiklenir. Kareyi dinleyen taraf Dispose eder.</summary>
    public event EventHandler<Mat>? FrameReady;

    /// <summary>Kamera koptuğunda veya okuma hatasında tetiklenir; döngü durmuş olur. Mesaj Türkçedir.</summary>
    public event EventHandler<string>? Error;

    /// <summary>Kare döngüsü çalışıyor mu?</summary>
    public bool IsRunning => _loop is { IsCompleted: false };

    /// <summary>Açık kameranın indeksi, kapalıysa -1.</summary>
    public int CameraIndex { get; private set; } = -1;

    /// <summary>Kamerayı açıp kare döngüsünü başlatır. index &lt; 0 ise 0-3 arasında çalışan ilk kamerayı bulur. Açılan indeksi döndürür.</summary>
    public int Start(int index = -1)
    {
        Stop();
        int first = index < 0 ? 0 : index;
        int last = index < 0 ? MaxCameraIndex : index;
        for (int i = first; i <= last && _capture == null; i++)
        {
            _capture = TryOpen(i);
            if (_capture != null) CameraIndex = i;
        }

        if (_capture == null)
            throw new InvalidOperationException(index < 0
                ? $"0-{MaxCameraIndex} arasındaki indekslerde çalışan kamera bulunamadı."
                : $"{index} numaralı kamera açılamadı.");

        _cts = new CancellationTokenSource();
        var capture = _capture;
        var token = _cts.Token;
        _loop = Task.Run(() => ReadLoop(capture, token));
        return CameraIndex;
    }

    /// <summary>Kare döngüsünü durdurur ve kamerayı serbest bırakır.</summary>
    public void Stop()
    {
        _cts?.Cancel();
        try
        {
            _loop?.Wait();
        }
        catch (AggregateException)
        {
            // Döngüdeki hata zaten Error olayıyla bildirildi.
        }
        _capture?.Dispose();
        _cts?.Dispose();
        _capture = null;
        _cts = null;
        _loop = null;
        CameraIndex = -1;
    }

    public void Dispose() => Stop();

    /// <summary>Kamerayı MSMF ile 1280x720 açar. İlk kareler gelmezse null döner.</summary>
    private static VideoCapture? TryOpen(int index)
    {
        var capture = new VideoCapture(index, VideoCaptureAPIs.MSMF);
        if (capture.IsOpened())
        {
            // Iriun gibi sanal kameralar varsayılan 640x480'de bozuk renk veriyor, 1280x720 istemek düzeltiyor.
            capture.FrameWidth = 1280;
            capture.FrameHeight = 720;
            using var test = new Mat();
            for (int i = 0; i < 10; i++)
                if (capture.Read(test) && !test.Empty()) return capture;
        }
        capture.Dispose();
        return null;
    }

    private void ReadLoop(VideoCapture capture, CancellationToken token)
    {
        int failures = 0;
        try
        {
            while (!token.IsCancellationRequested)
            {
                var frame = new Mat();
                if (capture.Read(frame) && !frame.Empty())
                {
                    failures = 0;
                    if (FrameReady is { } handler) handler(this, frame);
                    else frame.Dispose();
                    continue;
                }

                frame.Dispose();
                if (++failures >= MaxReadFailures)
                {
                    Error?.Invoke(this, "Kameradan görüntü alınamıyor. Bağlantıyı kontrol edip kamerayı yeniden başlatın.");
                    return;
                }
                Thread.Sleep(30);
            }
        }
        catch (Exception ex)
        {
            if (!token.IsCancellationRequested)
                Error?.Invoke(this, "Kamera hatası: " + ex.Message);
        }
    }
}
