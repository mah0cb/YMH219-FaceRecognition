namespace FaceRecognitionApp;

static class Program
{
    /// <summary>Uygulamanın giriş noktası.</summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Beklenmeyen bir hata olursa uygulama kapanmasın, kullanıcıya Türkçe bildirilsin.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
            MessageBox.Show("Beklenmeyen bir hata oluştu:\n" + e.Exception.Message, "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        Application.Run(new MainForm());
    }
}
