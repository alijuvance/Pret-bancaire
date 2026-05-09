using PretBancaire.Models;
using PretBancaire.Services;
using PretBancaire.Utils;

namespace PretBancaire.Forms
{
    /// <summary>
    /// Formulaire de gestion des clients (CRUD complet).
    /// </summary>
    public class FormClients : UserControl
    {
        private readonly ClientService _service = new();
        private DataGridView dgv = null!;
        private TextBox txtRecherche = null!, txtNom = null!, txtPrenom = null!;
        private TextBox txtCin = null!, txtTel = null!, txtAdresse = null!, txtEmail = null!;
        private DateTimePicker dtpNaissance = null!;
        private int? _selectedId = null;

        public FormClients()
        {
            this.BackColor = Color.FromArgb(24, 28, 40);
            ConstruireInterface();
            ChargerDonnees();
        }

        private void ConstruireInterface()
        {
            // === Barre de recherche ===
            var panelSearch = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent };

            txtRecherche = new TextBox
            {
                PlaceholderText = "🔍 Rechercher par nom, prénom ou CIN...",
                Font = new Font("Segoe UI", 11),
                Size = new Size(350, 30),
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(45, 52, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtRecherche.TextChanged += (s, e) => Rechercher();
            panelSearch.Controls.Add(txtRecherche);

            var btnNouveau = CreerBouton("➕ Nouveau", Color.FromArgb(34, 197, 94), 380, 10);
            btnNouveau.Click += (s, e) => NouveauClient();
            panelSearch.Controls.Add(btnNouveau);

            this.Controls.Add(panelSearch);

            // === Grille de données ===
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(28, 32, 46),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(50, 55, 70),
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10)
            };
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(32, 38, 55);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 130, 246);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(16, 20, 32);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(150, 160, 180);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 40;
            dgv.RowTemplate.Height = 35;
            dgv.SelectionChanged += (s, e) => SelectionChanged();
            this.Controls.Add(dgv);

            // === Panel de formulaire en bas ===
            var panelForm = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 170,
                BackColor = Color.FromArgb(28, 32, 46),
                Padding = new Padding(15)
            };

            int x = 15, y = 10;
            panelForm.Controls.Add(CreerLabel("Nom:", x, y)); txtNom = CreerTextBox(x + 55, y, 150); panelForm.Controls.Add(txtNom);
            panelForm.Controls.Add(CreerLabel("Prénom:", x + 220, y)); txtPrenom = CreerTextBox(x + 290, y, 150); panelForm.Controls.Add(txtPrenom);
            panelForm.Controls.Add(CreerLabel("CIN:", x + 460, y)); txtCin = CreerTextBox(x + 510, y, 130); panelForm.Controls.Add(txtCin);

            y = 50;
            panelForm.Controls.Add(CreerLabel("Naissance:", x, y));
            dtpNaissance = new DateTimePicker
            {
                Location = new Point(x + 80, y),
                Size = new Size(140, 28),
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short
            };
            panelForm.Controls.Add(dtpNaissance);

            panelForm.Controls.Add(CreerLabel("Tél:", x + 240, y)); txtTel = CreerTextBox(x + 280, y, 150); panelForm.Controls.Add(txtTel);
            panelForm.Controls.Add(CreerLabel("Email:", x + 450, y)); txtEmail = CreerTextBox(x + 500, y, 180); panelForm.Controls.Add(txtEmail);

            y = 90;
            panelForm.Controls.Add(CreerLabel("Adresse:", x, y)); txtAdresse = CreerTextBox(x + 70, y, 350); panelForm.Controls.Add(txtAdresse);

            // Boutons d'action
            y = 125;
            var btnSauver = CreerBouton("💾 Enregistrer", Color.FromArgb(60, 130, 246), x + 450, y);
            btnSauver.Click += (s, e) => Sauvegarder();
            panelForm.Controls.Add(btnSauver);

            var btnSuppr = CreerBouton("🗑️ Supprimer", Color.FromArgb(239, 68, 68), x + 590, y);
            btnSuppr.Click += (s, e) => Supprimer();
            panelForm.Controls.Add(btnSuppr);

            var btnClear = CreerBouton("🔄 Nouveau", Color.FromArgb(107, 114, 128), x + 730, y);
            btnClear.Click += (s, e) => NouveauClient();
            panelForm.Controls.Add(btnClear);

            this.Controls.Add(panelForm);
        }

        private void ChargerDonnees()
        {
            try
            {
                var clients = _service.GetTousClients();
                dgv.DataSource = null;
                dgv.Columns.Clear();
                dgv.DataSource = clients;

                // Configurer les colonnes visibles
                if (dgv.Columns.Count > 0)
                {
                    dgv.Columns["Id"].HeaderText = "ID";
                    dgv.Columns["Id"].Width = 50;
                    dgv.Columns["Nom"].HeaderText = "Nom";
                    dgv.Columns["Prenom"].HeaderText = "Prénom";
                    dgv.Columns["Cin"].HeaderText = "CIN";
                    dgv.Columns["Telephone"].HeaderText = "Téléphone";
                    dgv.Columns["Email"].HeaderText = "Email";
                    dgv.Columns["DateNaissance"].HeaderText = "Naissance";
                    dgv.Columns["DateNaissance"].DefaultCellStyle.Format = "dd/MM/yyyy";

                    // Masquer certaines colonnes
                    if (dgv.Columns.Contains("Adresse")) dgv.Columns["Adresse"].Visible = false;
                    if (dgv.Columns.Contains("DateInscription")) dgv.Columns["DateInscription"].Visible = false;
                    if (dgv.Columns.Contains("Actif")) dgv.Columns["Actif"].Visible = false;
                    if (dgv.Columns.Contains("NomComplet")) dgv.Columns["NomComplet"].Visible = false;
                    if (dgv.Columns.Contains("Age")) dgv.Columns["Age"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Rechercher()
        {
            try
            {
                var terme = txtRecherche.Text.Trim();
                var clients = string.IsNullOrEmpty(terme) ? _service.GetTousClients() : _service.RechercherClients(terme);
                dgv.DataSource = clients;
            }
            catch { }
        }

        private void SelectionChanged()
        {
            if (dgv.CurrentRow?.DataBoundItem is Client c)
            {
                _selectedId = c.Id;
                txtNom.Text = c.Nom;
                txtPrenom.Text = c.Prenom;
                txtCin.Text = c.Cin;
                txtTel.Text = c.Telephone;
                txtEmail.Text = c.Email;
                txtAdresse.Text = c.Adresse;
                dtpNaissance.Value = c.DateNaissance;
            }
        }

        private void Sauvegarder()
        {
            var erreurs = ValidationHelper.ValiderClient(txtNom.Text, txtPrenom.Text, txtCin.Text, txtTel.Text, txtEmail.Text);
            if (erreurs.Count > 0)
            {
                MessageBox.Show(string.Join("\n", erreurs), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var client = new Client
                {
                    Id = _selectedId ?? 0,
                    Nom = txtNom.Text.Trim(),
                    Prenom = txtPrenom.Text.Trim(),
                    Cin = txtCin.Text.Trim(),
                    Telephone = txtTel.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Adresse = txtAdresse.Text.Trim(),
                    DateNaissance = dtpNaissance.Value
                };

                // Vérifier l'unicité du CIN
                if (_service.CinExiste(client.Cin, _selectedId))
                {
                    MessageBox.Show("Ce CIN est déjà utilisé par un autre client.", "Doublon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_selectedId.HasValue)
                    _service.ModifierClient(client);
                else
                    _service.AjouterClient(client);

                ChargerDonnees();
                NouveauClient();
                MessageBox.Show("Client enregistré avec succès !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Supprimer()
        {
            if (!_selectedId.HasValue) return;
            if (MessageBox.Show("Voulez-vous vraiment supprimer ce client ?", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            var (success, message) = _service.SupprimerClient(_selectedId.Value);
            MessageBox.Show(message, success ? "Succès" : "Erreur",
                MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (success) { ChargerDonnees(); NouveauClient(); }
        }

        private void NouveauClient()
        {
            _selectedId = null;
            txtNom.Clear(); txtPrenom.Clear(); txtCin.Clear();
            txtTel.Clear(); txtEmail.Clear(); txtAdresse.Clear();
            dtpNaissance.Value = DateTime.Today.AddYears(-25);
        }

        // === Helpers UI ===
        private static Label CreerLabel(string text, int x, int y) => new()
        {
            Text = text, Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(200, 210, 225),
            Location = new Point(x, y + 3), AutoSize = true
        };

        private static TextBox CreerTextBox(int x, int y, int width) => new()
        {
            Font = new Font("Segoe UI", 10), Size = new Size(width, 28),
            Location = new Point(x, y),
            BackColor = Color.FromArgb(45, 52, 70),
            ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle
        };

        private static Button CreerBouton(string text, Color bgColor, int x, int y) 
        {
            var btn = new Button
            {
                Text = text, Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(130, 35), Location = new Point(x, y),
                BackColor = bgColor, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
