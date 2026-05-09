using PretBancaire.Models;
using PretBancaire.Services;

namespace PretBancaire.Forms
{
    /// <summary>
    /// Formulaire de gestion des utilisateurs (Admin uniquement).
    /// </summary>
    public class FormUtilisateurs : UserControl
    {
        private readonly AuthService _service = new();
        private DataGridView dgv = null!;
        private TextBox txtNom = null!, txtPrenom = null!, txtLogin = null!, txtMdp = null!;
        private ComboBox cmbRole = null!;
        private int? _selectedId = null;

        public FormUtilisateurs()
        {
            this.BackColor = Color.FromArgb(24, 28, 40);
            ConstruireInterface();
            ChargerDonnees();
        }

        private void ConstruireInterface()
        {
            // === Grille ===
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(28, 32, 46), ForeColor = Color.White,
                GridColor = Color.FromArgb(50, 55, 70), BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10)
            };
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(32, 38, 55);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 130, 246);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(16, 20, 32);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(150, 160, 180);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 40; dgv.RowTemplate.Height = 35;
            dgv.SelectionChanged += (s, e) => SelectionChanged();
            this.Controls.Add(dgv);

            // === Formulaire ===
            var panelForm = new Panel
            {
                Dock = DockStyle.Bottom, Height = 130,
                BackColor = Color.FromArgb(28, 32, 46), Padding = new Padding(15)
            };

            int x = 15, y = 10;
            panelForm.Controls.Add(Lbl("Nom:", x, y)); txtNom = Txt(x + 50, y, 150); panelForm.Controls.Add(txtNom);
            panelForm.Controls.Add(Lbl("Prénom:", x + 215, y)); txtPrenom = Txt(x + 280, y, 150); panelForm.Controls.Add(txtPrenom);
            panelForm.Controls.Add(Lbl("Rôle:", x + 450, y));
            cmbRole = new ComboBox
            {
                Font = new Font("Segoe UI", 10), Size = new Size(120, 28),
                Location = new Point(x + 500, y), DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 52, 70), ForeColor = Color.White
            };
            cmbRole.Items.AddRange(new[] { "Agent", "Admin" });
            cmbRole.SelectedIndex = 0;
            panelForm.Controls.Add(cmbRole);

            y = 50;
            panelForm.Controls.Add(Lbl("Login:", x, y)); txtLogin = Txt(x + 55, y, 150); panelForm.Controls.Add(txtLogin);
            panelForm.Controls.Add(Lbl("Mot de passe:", x + 215, y)); txtMdp = Txt(x + 330, y, 150); panelForm.Controls.Add(txtMdp);
            txtMdp.PasswordChar = '●';

            y = 90;
            var btnSave = Btn("💾 Enregistrer", Color.FromArgb(60, 130, 246), x + 350, y);
            btnSave.Click += (s, e) => Sauvegarder();
            panelForm.Controls.Add(btnSave);

            var btnDel = Btn("🗑️ Supprimer", Color.FromArgb(239, 68, 68), x + 490, y);
            btnDel.Click += (s, e) => Supprimer();
            panelForm.Controls.Add(btnDel);

            var btnNew = Btn("🔄 Nouveau", Color.FromArgb(107, 114, 128), x + 630, y);
            btnNew.Click += (s, e) => Nouveau();
            panelForm.Controls.Add(btnNew);

            this.Controls.Add(panelForm);
        }

        private void ChargerDonnees()
        {
            try
            {
                dgv.DataSource = null; dgv.Columns.Clear();
                dgv.AutoGenerateColumns = true;
                dgv.DataSource = _service.GetTousUtilisateurs();

                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    switch (col.Name)
                    {
                        case "Id": col.HeaderText = "ID"; col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; col.Width = 50; break;
                        case "Nom": col.HeaderText = "Nom"; break;
                        case "Prenom": col.HeaderText = "Prénom"; break;
                        case "Login": col.HeaderText = "Login"; break;
                        case "Role": col.HeaderText = "Rôle"; break;
                        case "DateCreation": col.HeaderText = "Créé le"; col.DefaultCellStyle.Format = "dd/MM/yyyy"; break;
                        case "Actif": col.HeaderText = "Actif"; break;
                        case "MotDePasse":
                        case "NomComplet":
                        case "EstAdmin":
                            col.Visible = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.StackTrace}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectionChanged()
        {
            try
            {
                if (dgv.CurrentRow?.DataBoundItem is Utilisateur u)
                {
                    _selectedId = u.Id;
                    txtNom.Text = u.Nom; txtPrenom.Text = u.Prenom;
                    txtLogin.Text = u.Login;
                    cmbRole.SelectedItem = u.Role;
                    txtMdp.Clear();
                }
            }
            catch { }
        }

        private void Sauvegarder()
        {
            if (string.IsNullOrWhiteSpace(txtNom.Text) || string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                MessageBox.Show("Nom et login sont obligatoires.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_service.LoginExiste(txtLogin.Text.Trim(), _selectedId))
            {
                MessageBox.Show("Ce login est déjà utilisé.", "Doublon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_selectedId.HasValue)
                {
                    var u = new Utilisateur
                    {
                        Id = _selectedId.Value, Nom = txtNom.Text.Trim(),
                        Prenom = txtPrenom.Text.Trim(), Login = txtLogin.Text.Trim(),
                        Role = cmbRole.SelectedItem?.ToString() ?? "Agent"
                    };
                    _service.ModifierUtilisateur(u);

                    if (!string.IsNullOrWhiteSpace(txtMdp.Text))
                        _service.ModifierMotDePasse(_selectedId.Value, txtMdp.Text);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(txtMdp.Text))
                    {
                        MessageBox.Show("Le mot de passe est obligatoire pour un nouvel utilisateur.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    var u = new Utilisateur
                    {
                        Nom = txtNom.Text.Trim(), Prenom = txtPrenom.Text.Trim(),
                        Login = txtLogin.Text.Trim(), MotDePasse = txtMdp.Text,
                        Role = cmbRole.SelectedItem?.ToString() ?? "Agent"
                    };
                    _service.AjouterUtilisateur(u);
                }

                ChargerDonnees(); Nouveau();
                MessageBox.Show("Utilisateur enregistré !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.StackTrace}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Supprimer()
        {
            if (!_selectedId.HasValue) return;
            if (MessageBox.Show("Supprimer cet utilisateur ?", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _service.SupprimerUtilisateur(_selectedId.Value);
            ChargerDonnees(); Nouveau();
        }

        private void Nouveau()
        {
            _selectedId = null;
            txtNom.Clear(); txtPrenom.Clear(); txtLogin.Clear(); txtMdp.Clear();
            cmbRole.SelectedIndex = 0;
        }

        private static Label Lbl(string t, int x, int y) => new()
        { Text = t, Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(200, 210, 225), Location = new Point(x, y + 3), AutoSize = true };

        private static TextBox Txt(int x, int y, int w) => new()
        { Font = new Font("Segoe UI", 10), Size = new Size(w, 28), Location = new Point(x, y), BackColor = Color.FromArgb(45, 52, 70), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        private static Button Btn(string t, Color bg, int x, int y)
        { var b = new Button { Text = t, Font = new Font("Segoe UI", 10, FontStyle.Bold), Size = new Size(130, 35), Location = new Point(x, y), BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; return b; }
    }
}


