using PretBancaire.Models;
using PretBancaire.Services;
using PretBancaire.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PretBancaire.Forms
{
    public class FormClients : UserControl
    {
        private readonly ClientService _service = new();
        private DataGridView dgv = null!;
        private TextBox txtRecherche = null!, txtNom = null!, txtPrenom = null!;
        private TextBox txtCin = null!, txtTel = null!, txtAdresse = null!, txtEmail = null!;
        private Button btnNouveau = null!, btnEnregistrer = null!, btnModifier = null!, btnSupprimer = null!;
        private int? _selectedId = null;

        public FormClients()
        {
            this.BackColor = UIHelper.BgDark;
            ConstruireInterface();
            ChargerDonnees();
        }

        private void ConstruireInterface()
        {
            // === Barre de recherche ===
            var panelSearch = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.Transparent, Padding = new Padding(15, 10, 15, 0) };

            txtRecherche = new TextBox
            {
                PlaceholderText = "Rechercher par nom, prenom ou CIN...",
                Font = new Font("Segoe UI", 11),
                Size = new Size(380, 35),
                Location = new Point(15, 10),
                BackColor = UIHelper.BgInput,
                ForeColor = UIHelper.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtRecherche.TextChanged += (s, e) => Rechercher();
            panelSearch.Controls.Add(txtRecherche);
            this.Controls.Add(panelSearch);

            // === DataGridView ===
            dgv = new DataGridView();
            UIHelper.FormaterGrid(dgv);
            dgv.Dock = DockStyle.Fill;
            dgv.SelectionChanged += OnSelectionChanged;
            this.Controls.Add(dgv);

            // === Panel formulaire ===
            var panelForm = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 170,
                BackColor = UIHelper.BgCard,
                Padding = new Padding(20, 12, 20, 12)
            };

            var flowInputs = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 72,
                WrapContents = true,
                Padding = new Padding(0)
            };

            txtNom = UIHelper.CreerTextBox(170);
            txtPrenom = UIHelper.CreerTextBox(170);
            txtCin = UIHelper.CreerTextBox(130);
            txtTel = UIHelper.CreerTextBox(140);
            txtEmail = UIHelper.CreerTextBox(200);
            txtAdresse = UIHelper.CreerTextBox(200);

            flowInputs.Controls.Add(UIHelper.CreerChamp("Nom", txtNom, 170));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Prenom", txtPrenom, 170));
            flowInputs.Controls.Add(UIHelper.CreerChamp("CIN", txtCin, 130));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Telephone", txtTel, 140));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Email", txtEmail, 200));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Adresse", txtAdresse, 200));
            panelForm.Controls.Add(flowInputs);

            // === 4 boutons principaux ===
            var flowBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };

            btnNouveau = UIHelper.CreerBouton("Nouveau", Color.FromArgb(100, 116, 139), 120);
            btnNouveau.Click += (s, e) => NouveauClient();

            btnEnregistrer = UIHelper.CreerBouton("Enregistrer", UIHelper.AccentBlue, 120);
            btnEnregistrer.Click += (s, e) => Ajouter();

            btnModifier = UIHelper.CreerBouton("Modifier", UIHelper.AccentOrange, 120);
            btnModifier.Click += (s, e) => Modifier();
            btnModifier.Enabled = false;

            btnSupprimer = UIHelper.CreerBouton("Supprimer", UIHelper.AccentRed, 120);
            btnSupprimer.Click += (s, e) => Supprimer();
            btnSupprimer.Enabled = false;

            flowBtns.Controls.Add(btnNouveau);
            flowBtns.Controls.Add(btnEnregistrer);
            flowBtns.Controls.Add(btnModifier);
            flowBtns.Controls.Add(btnSupprimer);

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

                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    switch (col.Name)
                    {
                        case "Id":
                            col.HeaderText = "ID";
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotIdentity);
                            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                            col.Width = 70;
                            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            break;
                        case "Nom": 
                            col.HeaderText = "Nom"; 
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotPerson);
                            break;
                        case "Prenom": 
                            col.HeaderText = "Pr\u00e9nom"; 
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotPerson);
                            break;
                        case "Cin": 
                            col.HeaderText = "CIN"; 
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotDocument);
                            break;
                        case "Telephone": 
                            col.HeaderText = "T\u00e9l\u00e9phone"; 
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotContact);
                            break;
                        case "Email": 
                            col.HeaderText = "Email"; 
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotContact);
                            break;
                        case "Adresse":
                        case "DateInscription":
                        case "Actif":
                        case "NomComplet":
                            col.Visible = false;
                            break;
                    }
                }

                dgv.SelectionChanged += OnSelectionChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur: " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Rechercher()
        {
            try
            {
                var terme = txtRecherche.Text.Trim();
                dgv.DataSource = string.IsNullOrEmpty(terme) ? _service.GetTousClients() : _service.RechercherClients(terme);
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
                if (btnModifier != null) btnModifier.Enabled = true;
                if (btnSupprimer != null) btnSupprimer.Enabled = true;
                if (btnEnregistrer != null) btnEnregistrer.Enabled = false;
            }
        }

        private void Ajouter()
        {
            if (_selectedId.HasValue) return;

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
                    Nom = txtNom.Text.Trim(),
                    Prenom = txtPrenom.Text.Trim(),
                    Cin = txtCin.Text.Trim(),
                    Telephone = txtTel.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Adresse = txtAdresse.Text.Trim()
                };

                if (_service.CinExiste(client.Cin, null))
                {
                    MessageBox.Show("Ce CIN est déjà utilisé.", "Doublon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _service.AjouterClient(client);
                ChargerDonnees();
                NouveauClient();
                MessageBox.Show("Client enregistré avec succès !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur: " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Modifier()
        {
            if (!_selectedId.HasValue) return;

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
                    Id = _selectedId.Value,
                    Nom = txtNom.Text.Trim(),
                    Prenom = txtPrenom.Text.Trim(),
                    Cin = txtCin.Text.Trim(),
                    Telephone = txtTel.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Adresse = txtAdresse.Text.Trim()
                };

                if (_service.CinExiste(client.Cin, _selectedId))
                {
                    MessageBox.Show("Ce CIN est déjà utilisé.", "Doublon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _service.ModifierClient(client);
                ChargerDonnees();
                NouveauClient();
                MessageBox.Show("Client modifié avec succès !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur: " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Supprimer()
        {
            if (!_selectedId.HasValue) { MessageBox.Show("Selectionnez un client.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show("Voulez-vous vraiment supprimer ce client ?", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            var (success, message) = _service.SupprimerClient(_selectedId.Value);
            MessageBox.Show(message, success ? "Succes" : "Erreur",
                MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (success) { ChargerDonnees(); NouveauClient(); }
        }

        private void NouveauClient()
        {
            _selectedId = null;
            txtNom.Clear(); txtPrenom.Clear(); txtCin.Clear();
            txtTel.Clear(); txtEmail.Clear(); txtAdresse.Clear();
            if (btnEnregistrer != null) btnEnregistrer.Enabled = true;
            if (btnModifier != null) btnModifier.Enabled = false;
            if (btnSupprimer != null) btnSupprimer.Enabled = false;
            dgv.ClearSelection();
        }
    }
}