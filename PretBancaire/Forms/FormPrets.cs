using PretBancaire.Models;
using PretBancaire.Services;
using PretBancaire.Utils;

namespace PretBancaire.Forms
{
    /// <summary>
    /// Formulaire de gestion des prêts bancaires.
    /// </summary>
    public class FormPrets : UserControl
    {
        private readonly PretService _service = new();
        private readonly ClientService _clientService = new();
        private DataGridView dgv = null!;
        private ComboBox cmbClient = null!, cmbStatutFiltre = null!;
        private TextBox txtMontant = null!, txtTaux = null!, txtDuree = null!, txtNotes = null!;
        private Label lblMensualite = null!, lblTotal = null!;

        public FormPrets()
        {
            this.BackColor = Color.FromArgb(24, 28, 40);
            ConstruireInterface();
            ChargerDonnees();
        }

        private void ConstruireInterface()
        {
            // === Barre de filtre ===
            var panelFiltre = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent };

            panelFiltre.Controls.Add(CreerLabel("Filtrer par statut:", 10, 15));
            cmbStatutFiltre = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(160, 28),
                Location = new Point(145, 12),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 52, 70),
                ForeColor = Color.White
            };
            cmbStatutFiltre.Items.AddRange(new[] { "Tous", "EnAttente", "Approuve", "EnCours", "Termine", "Rejete" });
            cmbStatutFiltre.SelectedIndex = 0;
            cmbStatutFiltre.SelectedIndexChanged += (s, e) => Filtrer();
            panelFiltre.Controls.Add(cmbStatutFiltre);

            this.Controls.Add(panelFiltre);

            // === Grille ===
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(28, 32, 46),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(50, 55, 70),
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true, AllowUserToAddRows = false,
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
            this.Controls.Add(dgv);

            // === Panel formulaire ===
            var panelForm = new Panel
            {
                Dock = DockStyle.Bottom, Height = 195,
                BackColor = Color.FromArgb(28, 32, 46), Padding = new Padding(15)
            };

            int x = 15, y = 10;
            panelForm.Controls.Add(CreerLabel("Client:", x, y));
            cmbClient = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(250, 28), Location = new Point(x + 65, y),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 52, 70), ForeColor = Color.White
            };
            panelForm.Controls.Add(cmbClient);

            panelForm.Controls.Add(CreerLabel("Montant:", x + 340, y));
            txtMontant = CreerTextBox(x + 415, y, 130); panelForm.Controls.Add(txtMontant);
            txtMontant.TextChanged += (s, e) => CalculerApercu();

            panelForm.Controls.Add(CreerLabel("USD", x + 550, y));

            y = 50;
            panelForm.Controls.Add(CreerLabel("Taux (%):", x, y));
            txtTaux = CreerTextBox(x + 80, y, 80); panelForm.Controls.Add(txtTaux);
            txtTaux.TextChanged += (s, e) => CalculerApercu();

            panelForm.Controls.Add(CreerLabel("Durée (mois):", x + 180, y));
            txtDuree = CreerTextBox(x + 300, y, 80); panelForm.Controls.Add(txtDuree);
            txtDuree.TextChanged += (s, e) => CalculerApercu();

            lblMensualite = new Label
            {
                Text = "Mensualité: — USD", Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 197, 94), Location = new Point(x + 420, y + 3), AutoSize = true
            };
            panelForm.Controls.Add(lblMensualite);

            y = 90;
            panelForm.Controls.Add(CreerLabel("Notes:", x, y));
            txtNotes = CreerTextBox(x + 65, y, 300); panelForm.Controls.Add(txtNotes);

            lblTotal = new Label
            {
                Text = "Total à rembourser: — USD", Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(168, 85, 247), Location = new Point(x + 420, y + 3), AutoSize = true
            };
            panelForm.Controls.Add(lblTotal);

            y = 135;
            var btnCreer = CreerBouton("💰 Créer le prêt", Color.FromArgb(60, 130, 246), x, y);
            btnCreer.Size = new Size(160, 38);
            btnCreer.Click += (s, e) => CreerPret();
            panelForm.Controls.Add(btnCreer);

            var btnApprouver = CreerBouton("✅ Approuver", Color.FromArgb(34, 197, 94), x + 180, y);
            btnApprouver.Click += (s, e) => ChangerStatut("Approuve");
            panelForm.Controls.Add(btnApprouver);

            var btnDemarrer = CreerBouton("▶ Démarrer", Color.FromArgb(59, 130, 246), x + 320, y);
            btnDemarrer.Click += (s, e) => ChangerStatut("EnCours");
            panelForm.Controls.Add(btnDemarrer);

            var btnRejeter = CreerBouton("❌ Rejeter", Color.FromArgb(239, 68, 68), x + 460, y);
            btnRejeter.Click += (s, e) => ChangerStatut("Rejete");
            panelForm.Controls.Add(btnRejeter);

            this.Controls.Add(panelForm);

            // Charger les clients dans le combobox
            ChargerClients();
        }

        private void ChargerClients()
        {
            try
            {
                var clients = _clientService.GetTousClients();
                cmbClient.Items.Clear();
                cmbClient.Items.Add(new ComboItem(0, "-- Sélectionner un client --"));
                foreach (var c in clients)
                    cmbClient.Items.Add(new ComboItem(c.Id, $"{c.NomComplet} (CIN: {c.Cin})"));
                cmbClient.SelectedIndex = 0;
            }
            catch { }
        }

        private void ChargerDonnees()
        {
            try
            {
                dgv.DataSource = null;
                dgv.Columns.Clear();
                dgv.AutoGenerateColumns = true;
                var prets = _service.GetTousPrets();
                dgv.DataSource = prets;

                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    switch (col.Name)
                    {
                        case "Id": col.HeaderText = "ID"; col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; col.Width = 50; break;
                        case "NomClient": col.HeaderText = "Client"; break;
                        case "Montant": col.HeaderText = "Montant"; col.DefaultCellStyle.Format = "N2"; break;
                        case "TauxInteret": col.HeaderText = "Taux %"; break;
                        case "DureeMois": col.HeaderText = "Durée"; break;
                        case "Mensualite": col.HeaderText = "Mensualité"; col.DefaultCellStyle.Format = "N2"; break;
                        case "MontantTotal": col.HeaderText = "Total"; col.DefaultCellStyle.Format = "N2"; break;
                        case "StatutLibelle": col.HeaderText = "Statut"; break;
                        case "DateDemande": col.HeaderText = "Date"; col.DefaultCellStyle.Format = "dd/MM/yyyy"; break;
                        case "ClientId":
                        case "Statut":
                        case "DateApprobation":
                        case "Notes":
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

        private void Filtrer()
        {
            try
            {
                string filtre = cmbStatutFiltre.SelectedItem?.ToString() ?? "Tous";
                dgv.DataSource = filtre == "Tous" ? _service.GetTousPrets() : _service.GetPretsByStatut(filtre);
            }
            catch { }
        }

        private void CalculerApercu()
        {
            if (decimal.TryParse(txtMontant.Text, out decimal m) &&
                decimal.TryParse(txtTaux.Text, out decimal t) &&
                int.TryParse(txtDuree.Text, out int d) && d > 0)
            {
                var mensualite = Pret.CalculerMensualite(m, t, d);
                var total = mensualite * d;
                lblMensualite.Text = $"Mensualité: {mensualite:N2} USD";
                lblTotal.Text = $"Total à rembourser: {total:N2} USD";
            }
            else
            {
                lblMensualite.Text = "Mensualité: — USD";
                lblTotal.Text = "Total à rembourser: — USD";
            }
        }

        private void CreerPret()
        {
            var clientItem = cmbClient.SelectedItem as ComboItem;
            int? clientId = clientItem?.Id > 0 ? clientItem.Id : null;

            var erreurs = ValidationHelper.ValiderPret(txtMontant.Text, txtTaux.Text, txtDuree.Text, clientId);
            if (erreurs.Count > 0)
            {
                MessageBox.Show(string.Join("\n", erreurs), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var (success, message, _) = _service.CreerPret(
                    clientId!.Value,
                    decimal.Parse(txtMontant.Text),
                    decimal.Parse(txtTaux.Text),
                    int.Parse(txtDuree.Text),
                    txtNotes.Text.Trim()
                );

                MessageBox.Show(message, success ? "Succès" : "Erreur",
                    MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                if (success)
                {
                    ChargerDonnees();
                    txtMontant.Clear(); txtTaux.Clear(); txtDuree.Clear(); txtNotes.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.StackTrace}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChangerStatut(string statut)
        {
            if (dgv.CurrentRow?.DataBoundItem is not Pret pret) return;
            try
            {
                bool ok = statut switch
                {
                    "Approuve" => _service.ApprouverPret(pret.Id),
                    "EnCours" => _service.DemarrerPret(pret.Id),
                    "Rejete" => _service.RejeterPret(pret.Id),
                    _ => false
                };
                if (ok) ChargerDonnees();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.StackTrace}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // === Helpers ===
        private static Label CreerLabel(string text, int x, int y) => new()
        { Text = text, Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(200, 210, 225), Location = new Point(x, y + 3), AutoSize = true };

        private static TextBox CreerTextBox(int x, int y, int w) => new()
        { Font = new Font("Segoe UI", 10), Size = new Size(w, 28), Location = new Point(x, y), BackColor = Color.FromArgb(45, 52, 70), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        private static Button CreerBouton(string text, Color bg, int x, int y)
        { var b = new Button { Text = text, Font = new Font("Segoe UI", 10, FontStyle.Bold), Size = new Size(130, 38), Location = new Point(x, y), BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; return b; }
    }

    /// <summary>
    /// Item pour ComboBox avec ID et texte.
    /// </summary>
    public class ComboItem
    {
        public int Id { get; set; }
        public string Display { get; set; }
        public ComboItem(int id, string display) { Id = id; Display = display; }
        public override string ToString() => Display;
    }
}


