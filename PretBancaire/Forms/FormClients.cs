﻿using PretBancaire.Models;
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
            this.BackColor = Color.FromArgb(0, 0, 0);
            ConstruireInterface();
            ChargerDonnees();
        }

        private void ConstruireInterface()
        {
            // === Barre de recherche ===
            var panelSearch = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent };

            txtRecherche = new TextBox
            {
                PlaceholderText = "ðŸ” Rechercher par nom, Prénom ou CIN...",
                Font = new Font("Segoe UI", 11),
                Size = new Size(350, 30),
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtRecherche.TextChanged += (s, e) => Rechercher();
            panelSearch.Controls.Add(txtRecherche);

            var btnNouveau = CreerBouton("➕ Nouveau", Color.FromArgb(34, 197, 94), 120);
            btnNouveau.Location = new Point(380, 5); // Position absolue pour la barre du haut
            btnNouveau.Click += (s, e) => NouveauClient();
            panelSearch.Controls.Add(btnNouveau);

            this.Controls.Add(panelSearch);

            // === Grille de données ===
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(0, 0, 0), // slate-900
                ForeColor = Color.White,
                GridColor = Color.FromArgb(20, 20, 20),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10)
            };
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(10, 10, 10); // slate-800
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 112, 243); // blue-500
            dgv.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 0, 0);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(161, 161, 170); // slate-400
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 50;
            dgv.RowTemplate.Height = 45;
            dgv.SelectionChanged += OnSelectionChanged;
            this.Controls.Add(dgv);

            // === Panel de formulaire en bas ===
            var panelForm = new Panel
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.FromArgb(10, 10, 10), // slate-800
                Padding = new Padding(20)
            };

            var flowInputs = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                Padding = new Padding(0, 0, 0, 10)
            };

            txtNom = CreerTextBox(200);
            txtPrenom = CreerTextBox(200);
            txtCin = CreerTextBox(150);
            txtTel = CreerTextBox(150);
            txtEmail = CreerTextBox(250);
            txtAdresse = CreerTextBox(300);

            dtpNaissance = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short,
                Height = 35
            };

            flowInputs.Controls.Add(CreerChamp("Nom", txtNom, 200));
            flowInputs.Controls.Add(CreerChamp("Prénom", txtPrenom, 200));
            flowInputs.Controls.Add(CreerChamp("CIN", txtCin, 150));
            flowInputs.Controls.Add(CreerChamp("Téléphone", txtTel, 150));
            flowInputs.Controls.Add(CreerChamp("Email", txtEmail, 250));
            flowInputs.Controls.Add(CreerChamp("Naissance", dtpNaissance, 150));
            flowInputs.Controls.Add(CreerChamp("Adresse", txtAdresse, 300));
            panelForm.Controls.Add(flowInputs);

            var flowBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 60,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 15, 0, 0)
            };

            var btnNouveauForm = CreerBouton("Nouveau", Color.FromArgb(100, 116, 139), 120);
            btnNouveauForm.Click += (s, e) => NouveauClient();
            var btnSupprimer = CreerBouton("Supprimer", Color.FromArgb(239, 68, 68), 120);
            btnSupprimer.Click += (s, e) => Supprimer();
            var btnEnregistrer = CreerBouton("Enregistrer", Color.FromArgb(0, 112, 243), 140);
            btnEnregistrer.Click += (s, e) => Sauvegarder();

            flowBtns.Controls.Add(btnNouveauForm);
            flowBtns.Controls.Add(btnSupprimer);
            flowBtns.Controls.Add(btnEnregistrer);
            panelForm.Controls.Add(flowBtns);

            this.Controls.Add(panelForm);
        }

        private void ChargerDonnees()
        {
            try
            {
                dgv.SelectionChanged -= OnSelectionChanged;
                var clients = _service.GetTousClients();
                dgv.DataSource = null;
                dgv.Columns.Clear();
                dgv.AutoGenerateColumns = true;
                dgv.DataSource = clients;

                // Configurer les colonnes visibles (accès null-safe)
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    switch (col.Name)
                    {
                        case "Id": col.HeaderText = "ID"; col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; col.Width = 50; break;
                        case "Nom": col.HeaderText = "Nom"; break;
                        case "Prenom": col.HeaderText = "Prénom"; break;
                        case "Cin": col.HeaderText = "CIN"; break;
                        case "Telephone": col.HeaderText = "Téléphone"; break;
                        case "Email": col.HeaderText = "Email"; break;
                        case "DateNaissance":
                            col.HeaderText = "Naissance";
                            col.DefaultCellStyle.Format = "dd/MM/yyyy";
                            break;
                        case "Adresse":
                        case "DateInscription":
                        case "Actif":
                        case "NomComplet":
                        case "Age":
                            col.Visible = false;
                            break;
                    }
                }
                dgv.SelectionChanged += OnSelectionChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.StackTrace}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void OnSelectionChanged(object? sender, EventArgs e)
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
                    MessageBox.Show("Ce CIN est déjÃ  utilisé par un autre client.", "Doublon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show($"Erreur: {ex.StackTrace}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private static TextBox CreerTextBox(int w) => new()
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(w, 35),
            BackColor = Color.FromArgb(20, 20, 20),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        private static Button CreerBouton(string text, Color bg, int w)
        {
            var btn = new Button
            {
                Text = text, Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(w, 40), Margin = new Padding(10, 0, 0, 0),
                BackColor = bg, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}





