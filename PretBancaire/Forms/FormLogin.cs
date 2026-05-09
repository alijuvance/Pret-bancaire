using PretBancaire.Services;

namespace PretBancaire.Forms
{
    public partial class FormLogin : Form
    {
        private readonly AuthService _authService = new();
        private int _tentatives = 0;

        public FormLogin()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // === Configuration du formulaire ===
            this.Text = "Système de Prêt Bancaire — Connexion";
            this.Size = new Size(480, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(24, 28, 40);

            // === Icône de l'application ===
            try
            {
                var icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Resources", "logo.ico");
                if (File.Exists(icoPath)) this.Icon = new Icon(icoPath);
            }
            catch { }

            // === Panel central ===
            var panelCenter = new Panel
            {
                Size = new Size(380, 380),
                Location = new Point(50, 40),
                BackColor = Color.FromArgb(32, 38, 55)
            };
            this.Controls.Add(panelCenter);

            // === Logo ===
            try
            {
                var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Resources", "logo.png");
                if (File.Exists(logoPath))
                {
                    var picLogo = new PictureBox
                    {
                        Image = Image.FromFile(logoPath),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Size = new Size(70, 70),
                        Location = new Point(155, 10),
                        BackColor = Color.Transparent
                    };
                    panelCenter.Controls.Add(picLogo);
                }
            }
            catch { }

            // === Titre ===
            var lblTitre = new Label
            {
                Text = "Prêt Bancaire",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 180, 255),
                AutoSize = false,
                Size = new Size(380, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 85)
            };
            panelCenter.Controls.Add(lblTitre);

            var lblSousTitre = new Label
            {
                Text = "Connectez-vous pour accéder au système",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(150, 160, 180),
                AutoSize = false,
                Size = new Size(380, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 125)
            };
            panelCenter.Controls.Add(lblSousTitre);

            // === Login ===
            var lblLogin = new Label
            {
                Text = "Identifiant",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(200, 210, 225),
                Location = new Point(40, 160),
                AutoSize = true
            };
            panelCenter.Controls.Add(lblLogin);

            var txtLogin = new TextBox
            {
                Name = "txtLogin",
                Font = new Font("Segoe UI", 11),
                Size = new Size(300, 30),
                Location = new Point(40, 183),
                BackColor = Color.FromArgb(45, 52, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            panelCenter.Controls.Add(txtLogin);

            // === Mot de passe ===
            var lblMdp = new Label
            {
                Text = "Mot de passe",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(200, 210, 225),
                Location = new Point(40, 220),
                AutoSize = true
            };
            panelCenter.Controls.Add(lblMdp);

            var txtMdp = new TextBox
            {
                Name = "txtMdp",
                Font = new Font("Segoe UI", 11),
                Size = new Size(300, 30),
                Location = new Point(40, 243),
                BackColor = Color.FromArgb(45, 52, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '●'
            };
            panelCenter.Controls.Add(txtMdp);

            // === Bouton Connexion ===
            var btnConnexion = new Button
            {
                Name = "btnConnexion",
                Text = "🔐 Se connecter",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(300, 40),
                Location = new Point(40, 290),
                BackColor = Color.FromArgb(60, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnConnexion.FlatAppearance.BorderSize = 0;
            btnConnexion.Click += (s, e) => SeConnecter(txtLogin.Text, txtMdp.Text);
            panelCenter.Controls.Add(btnConnexion);

            // === Label erreur ===
            var lblErreur = new Label
            {
                Name = "lblErreur",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(255, 100, 100),
                AutoSize = false,
                Size = new Size(300, 20),
                Location = new Point(40, 340),
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelCenter.Controls.Add(lblErreur);

            // === Touche Entrée ===
            this.AcceptButton = btnConnexion;

            this.ResumeLayout();
        }

        private void SeConnecter(string login, string motDePasse)
        {
            var lblErreur = this.Controls.Find("lblErreur", true).FirstOrDefault() as Label;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(motDePasse))
            {
                if (lblErreur != null) lblErreur.Text = "Veuillez remplir tous les champs.";
                return;
            }

            try
            {
                var user = _authService.SeConnecter(login, motDePasse);
                if (user != null)
                {
                    this.Hide();
                    var formMain = new FormMain();
                    formMain.FormClosed += (s, e) => this.Close();
                    formMain.Show();
                }
                else
                {
                    _tentatives++;
                    if (lblErreur != null)
                        lblErreur.Text = $"Identifiants incorrects ({3 - _tentatives} tentative(s) restante(s))";

                    if (_tentatives >= 3)
                    {
                        MessageBox.Show("Nombre maximum de tentatives atteint.", "Accès refusé",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de connexion à la base de données:\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
