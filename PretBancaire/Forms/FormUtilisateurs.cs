﻿using PretBancaire.Models;
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
            this.BackColor = Color.FromArgb(0, 0, 0);
            ConstruireInterface();
            ChargerDonnees();
        }

        private void ConstruireInterface()
        {
            // === Grille ===
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(0, 0, 0),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(20, 20, 20),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10)
            };
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(10, 10, 10);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 112, 243);
            dgv.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 0, 0);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(161, 161, 170);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 50; dgv.RowTemplate.Height = 45;
            dgv.SelectionChanged += (s, e) => SelectionChanged();
            this.Controls.Add(dgv);

            // === Formulaire ===
            var panelForm = new Panel
            {
                Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.FromArgb(10, 10, 10), Padding = new Padding(20)
            };

            var flowInputs = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(0, 0, 0, 10)
            };

            txtNom = CreerInputText();
            txtPrenom = CreerInputText();
            txtLogin = CreerInputText();
            txtMdp = CreerInputText();
            txtMdp.PasswordChar = '●';
            
            cmbRole = new ComboBox
            {
                Font = new Font("Segoe UI", 10), Height = 35,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(20, 20, 20), ForeColor = Color.White
            };
            cmbRole.Items.AddRange(new[] { "Agent", "Admin" });
            cmbRole.SelectedIndex = 0;

            flowInputs.Controls.Add(CreerChamp("Nom", txtNom, 200));
            flowInputs.Controls.Add(CreerChamp("Prénom", txtPrenom, 200));
            flowInputs.Controls.Add(CreerChamp("Login", txtLogin, 200));
            flowInputs.Controls.Add(CreerChamp("Mot de passe", txtMdp, 200));
            flowInputs.Controls.Add(CreerChamp("RÃ´le", cmbRole, 150));
            panelForm.Controls.Add(flowInputs);

            var flowBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 60, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 15, 0, 0)
            };

            var btnNew = CreerBouton("Nouveau", Color.FromArgb(100, 116, 139), 120);
            btnNew.Click += (s, e) => Nouveau();
            var btnDel = CreerBouton("Supprimer", Color.FromArgb(239, 68, 68), 120);
            btnDel.Click += (s, e) => Supprimer();
            var btnSave = CreerBouton("Enregistrer", Color.FromArgb(0, 112, 243), 140);
            btnSave.Click += (s, e) => Sauvegarder();

            flowBtns.Controls.Add(btnNew);
            flowBtns.Controls.Add(btnDel);
            flowBtns.Controls.Add(btnSave);
            panelForm.Controls.Add(flowBtns);

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
                        case "Role": col.HeaderText = "RÃ´le"; break;
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
                MessageBox.Show("Ce login est déjÃ  utilisé.", "Doublon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private Panel CreerChamp(string texteLabel, Control input, int largeur)
        {
            var p = new Panel { Width = largeur, Height = 65, Margin = new Padding(0, 0, 15, 15) };
            var lbl = new Label { Text = texteLabel.ToUpper(), Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(161, 161, 170), Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            input.Dock = DockStyle.Bottom;
            input.Height = 35;
            input.Font = new Font("Segoe UI", 10);
            p.Controls.Add(input);
            p.Controls.Add(lbl);
            return p;
        }

        private static TextBox CreerInputText() => new()
        {
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(20, 20, 20),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        private static Button CreerBouton(string text, Color bg, int w)
        {
            var b = new Button
            {
                Text = text, Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(w, 40), Margin = new Padding(10, 0, 0, 0),
                BackColor = bg, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}





