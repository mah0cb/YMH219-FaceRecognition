namespace FaceRecognitionApp;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        picGoruntu = new PictureBox();
        pnlKontroller = new Panel();
        grpKamera = new GroupBox();
        lblKamera = new Label();
        cmbKamera = new ComboBox();
        btnKameraBaslat = new Button();
        btnKameraDurdur = new Button();
        btnGoruntuSec = new Button();
        ofdGoruntu = new OpenFileDialog();
        btnYuzleriKaydet = new Button();
        grpVeriEkle = new GroupBox();
        lblEtiket = new Label();
        txtEtiket = new TextBox();
        btnKameradanOrnekAl = new Button();
        btnDosyalardanEkle = new Button();
        lblOrnekSayisi = new Label();
        ofdOrnekler = new OpenFileDialog();
        grpTanima = new GroupBox();
        btnModeliEgit = new Button();
        lblEsik = new Label();
        nudEsik = new NumericUpDown();
        lblEsikAciklama = new Label();
        lblSonuc = new Label();
        ssDurum = new StatusStrip();
        lblDurum = new ToolStripStatusLabel();
        ((System.ComponentModel.ISupportInitialize)picGoruntu).BeginInit();
        pnlKontroller.SuspendLayout();
        grpKamera.SuspendLayout();
        grpVeriEkle.SuspendLayout();
        grpTanima.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nudEsik).BeginInit();
        ssDurum.SuspendLayout();
        SuspendLayout();
        //
        // picGoruntu
        //
        picGoruntu.BackColor = Color.Black;
        picGoruntu.Dock = DockStyle.Fill;
        picGoruntu.Location = new Point(0, 0);
        picGoruntu.Name = "picGoruntu";
        picGoruntu.Size = new Size(884, 689);
        picGoruntu.SizeMode = PictureBoxSizeMode.Zoom;
        picGoruntu.TabIndex = 0;
        picGoruntu.TabStop = false;
        //
        // pnlKontroller
        //
        pnlKontroller.AutoScroll = true;
        pnlKontroller.Controls.Add(grpKamera);
        pnlKontroller.Controls.Add(grpVeriEkle);
        pnlKontroller.Controls.Add(grpTanima);
        pnlKontroller.Dock = DockStyle.Right;
        pnlKontroller.Location = new Point(884, 0);
        pnlKontroller.Name = "pnlKontroller";
        pnlKontroller.Padding = new Padding(8);
        pnlKontroller.Size = new Size(300, 689);
        pnlKontroller.TabIndex = 1;
        //
        // grpKamera
        //
        grpKamera.Controls.Add(lblKamera);
        grpKamera.Controls.Add(cmbKamera);
        grpKamera.Controls.Add(btnKameraBaslat);
        grpKamera.Controls.Add(btnKameraDurdur);
        grpKamera.Controls.Add(btnGoruntuSec);
        grpKamera.Controls.Add(btnYuzleriKaydet);
        grpKamera.Location = new Point(8, 8);
        grpKamera.Name = "grpKamera";
        grpKamera.Size = new Size(282, 172);
        grpKamera.TabIndex = 0;
        grpKamera.TabStop = false;
        grpKamera.Text = "Kamera / Görüntü";
        //
        // lblKamera
        //
        lblKamera.AutoSize = true;
        lblKamera.Location = new Point(12, 30);
        lblKamera.Name = "lblKamera";
        lblKamera.Size = new Size(98, 15);
        lblKamera.TabIndex = 0;
        lblKamera.Text = "Kamera indeksi:";
        //
        // cmbKamera
        //
        cmbKamera.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbKamera.FormattingEnabled = true;
        cmbKamera.Items.AddRange(new object[] { "Otomatik", "0", "1", "2", "3" });
        cmbKamera.Location = new Point(120, 26);
        cmbKamera.Name = "cmbKamera";
        cmbKamera.Size = new Size(150, 23);
        cmbKamera.TabIndex = 1;
        //
        // btnKameraBaslat
        //
        btnKameraBaslat.Location = new Point(12, 60);
        btnKameraBaslat.Name = "btnKameraBaslat";
        btnKameraBaslat.Size = new Size(125, 32);
        btnKameraBaslat.TabIndex = 2;
        btnKameraBaslat.Text = "Kamera Başlat";
        btnKameraBaslat.UseVisualStyleBackColor = true;
        btnKameraBaslat.Click += btnKameraBaslat_Click;
        //
        // btnKameraDurdur
        //
        btnKameraDurdur.Location = new Point(145, 60);
        btnKameraDurdur.Name = "btnKameraDurdur";
        btnKameraDurdur.Size = new Size(125, 32);
        btnKameraDurdur.TabIndex = 3;
        btnKameraDurdur.Text = "Kamera Durdur";
        btnKameraDurdur.UseVisualStyleBackColor = true;
        btnKameraDurdur.Click += btnKameraDurdur_Click;
        //
        // btnGoruntuSec
        //
        btnGoruntuSec.Location = new Point(12, 98);
        btnGoruntuSec.Name = "btnGoruntuSec";
        btnGoruntuSec.Size = new Size(258, 28);
        btnGoruntuSec.TabIndex = 4;
        btnGoruntuSec.Text = "Görüntü Seç...";
        btnGoruntuSec.UseVisualStyleBackColor = true;
        btnGoruntuSec.Click += btnGoruntuSec_Click;
        //
        // ofdGoruntu
        //
        ofdGoruntu.Filter = "Resim dosyaları|*.jpg;*.jpeg;*.png;*.bmp|Tüm dosyalar|*.*";
        ofdGoruntu.Title = "Görüntü Seç";
        // 
        // btnYuzleriKaydet
        // 
        btnYuzleriKaydet.Location = new Point(12, 132);
        btnYuzleriKaydet.Name = "btnYuzleriKaydet";
        btnYuzleriKaydet.Size = new Size(258, 28);
        btnYuzleriKaydet.TabIndex = 5;
        btnYuzleriKaydet.Text = "Algılanan Yüzleri Kaydet";
        btnYuzleriKaydet.UseVisualStyleBackColor = true;
        btnYuzleriKaydet.Click += btnYuzleriKaydet_Click;
        // 
        // grpVeriEkle
        // 
        grpVeriEkle.Controls.Add(lblEtiket);
        grpVeriEkle.Controls.Add(txtEtiket);
        grpVeriEkle.Controls.Add(btnKameradanOrnekAl);
        grpVeriEkle.Controls.Add(btnDosyalardanEkle);
        grpVeriEkle.Controls.Add(lblOrnekSayisi);
        grpVeriEkle.Location = new Point(8, 188);
        grpVeriEkle.Name = "grpVeriEkle";
        grpVeriEkle.Size = new Size(282, 160);
        grpVeriEkle.TabIndex = 1;
        grpVeriEkle.TabStop = false;
        grpVeriEkle.Text = "Veri Ekle";
        // 
        // lblEtiket
        // 
        lblEtiket.AutoSize = true;
        lblEtiket.Location = new Point(12, 30);
        lblEtiket.Name = "lblEtiket";
        lblEtiket.Size = new Size(63, 15);
        lblEtiket.TabIndex = 0;
        lblEtiket.Text = "Etiket adı:";
        // 
        // txtEtiket
        // 
        txtEtiket.Location = new Point(90, 26);
        txtEtiket.Name = "txtEtiket";
        txtEtiket.PlaceholderText = "örn. Mahmut";
        txtEtiket.Size = new Size(180, 23);
        txtEtiket.TabIndex = 1;
        txtEtiket.TextChanged += txtEtiket_TextChanged;
        // 
        // btnKameradanOrnekAl
        // 
        btnKameradanOrnekAl.Location = new Point(12, 58);
        btnKameradanOrnekAl.Name = "btnKameradanOrnekAl";
        btnKameradanOrnekAl.Size = new Size(258, 30);
        btnKameradanOrnekAl.TabIndex = 2;
        btnKameradanOrnekAl.Text = "Kameradan Örnek Al (20)";
        btnKameradanOrnekAl.UseVisualStyleBackColor = true;
        btnKameradanOrnekAl.Click += btnKameradanOrnekAl_Click;
        // 
        // btnDosyalardanEkle
        // 
        btnDosyalardanEkle.Location = new Point(12, 94);
        btnDosyalardanEkle.Name = "btnDosyalardanEkle";
        btnDosyalardanEkle.Size = new Size(258, 30);
        btnDosyalardanEkle.TabIndex = 3;
        btnDosyalardanEkle.Text = "Dosyalardan Ekle...";
        btnDosyalardanEkle.UseVisualStyleBackColor = true;
        btnDosyalardanEkle.Click += btnDosyalardanEkle_Click;
        // 
        // lblOrnekSayisi
        // 
        lblOrnekSayisi.AutoSize = true;
        lblOrnekSayisi.Location = new Point(12, 132);
        lblOrnekSayisi.Name = "lblOrnekSayisi";
        lblOrnekSayisi.Size = new Size(107, 15);
        lblOrnekSayisi.TabIndex = 4;
        lblOrnekSayisi.Text = "Etiket adı girin.";
        // 
        // ofdOrnekler
        // 
        ofdOrnekler.Filter = "Resim dosyaları|*.jpg;*.jpeg;*.png;*.bmp|Tüm dosyalar|*.*";
        ofdOrnekler.Multiselect = true;
        ofdOrnekler.Title = "Etikete Eklenecek Fotoğrafları Seç";
        // 
        // grpTanima
        // 
        grpTanima.Controls.Add(btnModeliEgit);
        grpTanima.Controls.Add(lblEsik);
        grpTanima.Controls.Add(nudEsik);
        grpTanima.Controls.Add(lblEsikAciklama);
        grpTanima.Controls.Add(lblSonuc);
        grpTanima.Location = new Point(8, 356);
        grpTanima.Name = "grpTanima";
        grpTanima.Size = new Size(282, 310);
        grpTanima.TabIndex = 2;
        grpTanima.TabStop = false;
        grpTanima.Text = "Tanıma";
        // 
        // btnModeliEgit
        // 
        btnModeliEgit.Location = new Point(12, 26);
        btnModeliEgit.Name = "btnModeliEgit";
        btnModeliEgit.Size = new Size(258, 32);
        btnModeliEgit.TabIndex = 0;
        btnModeliEgit.Text = "Modeli Eğit";
        btnModeliEgit.UseVisualStyleBackColor = true;
        btnModeliEgit.Click += btnModeliEgit_Click;
        // 
        // lblEsik
        // 
        lblEsik.AutoSize = true;
        lblEsik.Location = new Point(12, 72);
        lblEsik.Name = "lblEsik";
        lblEsik.Size = new Size(85, 15);
        lblEsik.TabIndex = 1;
        lblEsik.Text = "Eşik (mesafe):";
        // 
        // nudEsik
        // 
        nudEsik.Location = new Point(120, 68);
        nudEsik.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
        nudEsik.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        nudEsik.Name = "nudEsik";
        nudEsik.Size = new Size(150, 23);
        nudEsik.TabIndex = 2;
        nudEsik.Value = new decimal(new int[] { 70, 0, 0, 0 });
        nudEsik.ValueChanged += nudEsik_ValueChanged;
        // 
        // lblEsikAciklama
        // 
        lblEsikAciklama.ForeColor = SystemColors.GrayText;
        lblEsikAciklama.Location = new Point(12, 98);
        lblEsikAciklama.Name = "lblEsikAciklama";
        lblEsikAciklama.Size = new Size(258, 64);
        lblEsikAciklama.TabIndex = 3;
        lblEsikAciklama.Text = "LBPH güven skoru bir mesafedir: düşük değer daha iyi eşleşme demektir. Skor eşiğin üstündeyse yüz \"Bilinmiyor\" sayılır.";
        // 
        // lblSonuc
        // 
        lblSonuc.BorderStyle = BorderStyle.FixedSingle;
        lblSonuc.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblSonuc.Location = new Point(12, 168);
        lblSonuc.Name = "lblSonuc";
        lblSonuc.Padding = new Padding(4);
        lblSonuc.Size = new Size(258, 130);
        lblSonuc.TabIndex = 4;
        lblSonuc.Text = "Tahmin: -";
        //
        // ssDurum
        //
        ssDurum.Items.AddRange(new ToolStripItem[] { lblDurum });
        ssDurum.Location = new Point(0, 689);
        ssDurum.Name = "ssDurum";
        ssDurum.Size = new Size(1184, 22);
        ssDurum.TabIndex = 2;
        //
        // lblDurum
        //
        lblDurum.Name = "lblDurum";
        lblDurum.Size = new Size(39, 17);
        lblDurum.Text = "Hazır.";
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1184, 711);
        Controls.Add(picGoruntu);
        Controls.Add(pnlKontroller);
        Controls.Add(ssDurum);
        MinimumSize = new Size(900, 740);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Yüz Tanıma";
        FormClosing += MainForm_FormClosing;
        Load += MainForm_Load;
        ((System.ComponentModel.ISupportInitialize)picGoruntu).EndInit();
        pnlKontroller.ResumeLayout(false);
        grpKamera.ResumeLayout(false);
        grpKamera.PerformLayout();
        grpVeriEkle.ResumeLayout(false);
        grpVeriEkle.PerformLayout();
        grpTanima.ResumeLayout(false);
        grpTanima.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)nudEsik).EndInit();
        ssDurum.ResumeLayout(false);
        ssDurum.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private PictureBox picGoruntu;
    private Panel pnlKontroller;
    private GroupBox grpKamera;
    private Label lblKamera;
    private ComboBox cmbKamera;
    private Button btnKameraBaslat;
    private Button btnKameraDurdur;
    private Button btnGoruntuSec;
    private OpenFileDialog ofdGoruntu;
    private Button btnYuzleriKaydet;
    private GroupBox grpVeriEkle;
    private Label lblEtiket;
    private TextBox txtEtiket;
    private Button btnKameradanOrnekAl;
    private Button btnDosyalardanEkle;
    private Label lblOrnekSayisi;
    private OpenFileDialog ofdOrnekler;
    private GroupBox grpTanima;
    private Button btnModeliEgit;
    private Label lblEsik;
    private NumericUpDown nudEsik;
    private Label lblEsikAciklama;
    private Label lblSonuc;
    private StatusStrip ssDurum;
    private ToolStripStatusLabel lblDurum;
}
