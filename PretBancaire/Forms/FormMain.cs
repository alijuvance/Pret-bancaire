using PretBancaire.Services;

namespace PretBancaire.Forms
{
    /// <summary>
    /// Formulaire principal avec menu de navigation latéral.
    /// </summary>
    public partial class FormMain : Form
    {
        private Panel panelContent = null!;
        private Label lblTitreSection = null!;

        public FormMain()
        {
            InitializeComponent();
            AfficherDashboard();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // === Form ===
            this.Text = "Système de Prêt Bancaire";
            this.Size = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.MinimumSize = new Size(1000, 600);

            // === Icône ===
            try
            {
                var icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Resources", "logo.ico");
                if (File.Exists(icoPath)) this.Icon = new Icon(icoPath);
            }
            catch { }

            // === Sidebar ===
            var sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = Color.FromArgb(10, 15, 28)
            };
            this.Controls.Add(sidebar);

            // Logo image dans la sidebar
            var panelLogo = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.Transparent };
            try
            {
                var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Resources", "logo.png");
                if (File.Exists(logoPath))
                {
                    var picLogo = new PictureBox
                    {
                        Image = Image.FromFile(logoPath),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Size = new Size(40, 40),
                        Location = new Point(15, 15),
                        BackColor = Color.Transparent
                    };
                    panelLogo.Controls.Add(picLogo);
                }
            }
            catch { }
            var lblLogo = new Label
            {
                Text = "Prêt Bancaire",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 180, 255),
                Location = new Point(60, 22),
                AutoSize = true
            };
            panelLogo.Controls.Add(lblLogo);
            
            // Utilisateur connecté
            var user = AuthService.UtilisateurConnecte;
            var lblUser = new Label
            {
                Text = $"👤 {user?.NomComplet ?? "N/A"}\n    {user?.Role ?? ""}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(150, 160, 180),
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Séparateur
            var sep1 = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(30, 41, 59) };

            // Boutons menu (ajoutés en ordre inverse car Dock=Top empile du bas vers le haut)
            var menus = new (string text, string icon, Action action)[]
            {
                ("Tableau de bord", "📊", AfficherDashboard),
                ("Clients", "👥", AfficherClients),
                ("Prêts", "💰", AfficherPrets),
                ("Paiements", "💳", AfficherPaiements),
            };

            // Ajouter le menu Utilisateurs si Admin
            var menuList = new List<(string text, string icon, Action action)>(menus);
            if (user?.EstAdmin == true)
                menuList.Add(("Utilisateurs", "🔧", AfficherUtilisateurs));

            // Panel pour les boutons
            var panelMenus = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = menuList.Count * 48 + 60,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(15, 45, 10, 0),
                BackColor = Color.Transparent
            };
            
            // L'ordre d'ajout définit l'ordre d'affichage (du bas vers le haut)
            sidebar.Controls.Add(panelMenus);
            sidebar.Controls.Add(sep1);
            sidebar.Controls.Add(lblUser);
            sidebar.Controls.Add(panelLogo);


            foreach (var (text, icon, action) in menuList)
            {
                var btn = new Button
                {
                    Text = $"  {icon}  {text}",
                    Font = new Font("Segoe UI", 11),
                    Size = new Size(218, 42),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.FromArgb(200, 210, 225),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 2, 0, 2)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 50, 70);
                btn.Click += (s, e) => action();
                panelMenus.Controls.Add(btn);
            }

            // Bouton déconnexion en bas
            var btnDeconnexion = new Button
            {
                Text = "  🚪  Déconnexion",
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Bottom,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(255, 100, 100),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                Padding = new Padding(15, 0, 0, 0)
            };
            btnDeconnexion.FlatAppearance.BorderSize = 0;
            btnDeconnexion.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 30, 30);
            btnDeconnexion.Click += (s, e) =>
            {
                new AuthService().SeDeconnecter();
                this.Close();
            };
            sidebar.Controls.Add(btnDeconnexion);

            // === Header ===
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(20, 0, 20, 0)
            };
            this.Controls.Add(header);

            lblTitreSection = new Label
            {
                Text = "📊 Tableau de bord",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Left,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(lblTitreSection);

            // === Zone de contenu ===
            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(20)
            };
            this.Controls.Add(panelContent);

            // Ordre d'ajout important pour le docking
            // En WinForms, les contrôles avec l'index le plus élevé (au fond) sont dockés en premier.
            // On veut : Sidebar dockée en premier (gauche), puis Header (Haut), puis Content (Fill).
            sidebar.SendToBack();
            panelContent.BringToFront();
            
            this.ResumeLayout();
        }

        private void SetContent(Control control, string titre)
        {
            panelContent.Controls.Clear();
            lblTitreSection.Text = titre;
            control.Dock = DockStyle.Fill;
            panelContent.Controls.Add(control);
        }

        private void AfficherDashboard()
        {
            SetContent(new FormDashboard(), "📊 Tableau de bord");
        }

        private void AfficherClients()
        {
            SetContent(new FormClients(), "👥 Gestion des Clients");
        }

        private void AfficherPrets()
        {
            SetContent(new FormPrets(), "💰 Gestion des Prêts");
        }

        private void AfficherPaiements()
        {
            SetContent(new FormPaiements(), "💳 Gestion des Paiements");
        }

        private void AfficherUtilisateurs()
        {
            SetContent(new FormUtilisateurs(), "🔧 Gestion des Utilisateurs");
        }
    }
}


