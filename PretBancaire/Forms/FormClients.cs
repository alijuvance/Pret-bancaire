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
        private DateTimePicker dtpNaissance = null!;
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

            dtpNaissance = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short,
                Height = 34
            };

            flowInputs.Controls.Add(UIHelper.CreerChamp("Nom", txtNom, 170));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Prenom", txtPrenom, 170));
            flowInputs.Controls.Add(UIHelper.CreerChamp("CIN", txtCin, 130));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Telephone", txtTel, 140));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Email", txtEmail, 200));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Naissance", dtpNaissance, 130));
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

            var btnNouveau = UIHelper.CreerBouton("Nouveau", Color.FromArgb(100, 116, 139), 120);
            btnNouveau.Click += (s, e) => NouveauClient();

            var btnEnregistrer = UIHelper.CreerBouton("Enregistrer", UIHelper.AccentBlue, 140);
            btnEnregistrer.Click += (s, e) => Sauvegarder();

            var btnModifier = UIHelper.CreerBouton("Modifier", UIHelper.AccentGreen, 130);
            btnModifier.Click += (s, e) => { if (_selectedId.HasValue) txtNom.Focus(); else MessageBox.Show("Selectionnez un client.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); };

            var btnSupprimer = UIHelper.CreerBouton("Supprimer", UIHelper.AccentRed, 120);
            btnSupprimer.Click += (s, e) => Supprimer();

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
                        case "DateNaissance":
                            col.HeaderText = "Naissance";
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotDate);
                            col.DefaultCellStyle.Format = "dd/MM/yyyy";
                            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
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

                if (_service.CinExiste(client.Cin, _selectedId))
                {
                    MessageBox.Show("Ce CIN est deja utilise par un autre client.", "Doublon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_selectedId.HasValue)
                    _service.ModifierClient(client);
                else
                    _service.AjouterClient(client);

                ChargerDonnees();
                NouveauClient();
                MessageBox.Show("Client enregistre avec succes !", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            dtpNaissance.Value = DateTime.Today.AddYears(-25);
            dgv.ClearSelection();
        }
    }
}