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
        ssDurum = new StatusStrip();
        lblDurum = new ToolStripStatusLabel();
        ((System.ComponentModel.ISupportInitialize)picGoruntu).BeginInit();
        pnlKontroller.SuspendLayout();
        grpKamera.SuspendLayout();
        grpVeriEkle.SuspendLayout();
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
        MinimumSize = new Size(900, 600);
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
    private StatusStrip ssDurum;
    private ToolStripStatusLabel lblDurum;
}
