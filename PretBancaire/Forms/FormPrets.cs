using PretBancaire.Models;
using PretBancaire.Services;
using PretBancaire.Utils;

namespace PretBancaire.Forms
{
    /// <summary>
    /// Formulaire de gestion des Prêts bancaires.
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
            this.BackColor = Color.FromArgb(0, 0, 0);
            ConstruireInterface();
            ChargerDonnees();
        }

        private void ConstruireInterface()
        {
            // === Barre de filtre ===
            var panelFiltre = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(0, 0, 0) };
            
            var lblFiltre = new Label { Text = "FILTRER PAR STATUT", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(161, 161, 170), AutoSize = true, Location = new Point(20, 22) };
            panelFiltre.Controls.Add(lblFiltre);

            cmbStatutFiltre = new ComboBox
            {
                Font = new Font("Segoe UI", 10), Size = new Size(160, 35), Location = new Point(160, 15),
                DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(20, 20, 20), ForeColor = Color.White
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
                BackgroundColor = Color.FromArgb(0, 0, 0), ForeColor = Color.White,
                GridColor = Color.FromArgb(20, 20, 20), BorderStyle = BorderStyle.None,
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
            this.Controls.Add(dgv);

            // === Panel formulaire ===
            var panelForm = new Panel
            {
                Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.FromArgb(10, 10, 10), Padding = new Padding(20)
            };

            var flowInputs = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(0, 0, 0, 10)
            };

            cmbClient = new ComboBox
            {
                Font = new Font("Segoe UI", 10), Height = 35, DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(20, 20, 20), ForeColor = Color.White
            };

            txtMontant = CreerInputText(); txtMontant.TextChanged += (s, e) => CalculerApercu();
            txtTaux = CreerInputText(); txtTaux.TextChanged += (s, e) => CalculerApercu();
            txtDuree = CreerInputText(); txtDuree.TextChanged += (s, e) => CalculerApercu();
            txtNotes = CreerInputText();

            flowInputs.Controls.Add(CreerChamp("Client", cmbClient, 250));
            flowInputs.Controls.Add(CreerChamp("Montant (USD)", txtMontant, 150));
            flowInputs.Controls.Add(CreerChamp("Taux (%)", txtTaux, 100));
            flowInputs.Controls.Add(CreerChamp("Durée (mois)", txtDuree, 100));
            flowInputs.Controls.Add(CreerChamp("Notes", txtNotes, 250));

            lblMensualite = new Label
            {
                Text = "Mensualité: — USD", Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 197, 94), AutoSize = true, Margin = new Padding(20, 10, 20, 0)
            };
            flowInputs.Controls.Add(lblMensualite);

            lblTotal = new Label
            {
                Text = "Total à rembourser: — USD", Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(168, 85, 247), AutoSize = true, Margin = new Padding(0, 12, 0, 0)
            };
            flowInputs.Controls.Add(lblTotal);
            panelForm.Controls.Add(flowInputs);

            var flowBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 60, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 15, 0, 0)
            };

            var btnCreer = CreerBouton("Créer le Prêt", Color.FromArgb(0, 112, 243), 160);
            btnCreer.Click += (s, e) => CreerPret();
            var btnApprouver = CreerBouton("Approuver", Color.FromArgb(34, 197, 94), 140);
            btnApprouver.Click += (s, e) => ChangerStatut("Approuve");
            var btnDemarrer = CreerBouton("Démarrer", Color.FromArgb(168, 85, 247), 140);
            btnDemarrer.Click += (s, e) => ChangerStatut("EnCours");
            var btnRejeter = CreerBouton("Rejeter", Color.FromArgb(239, 68, 68), 140);
            btnRejeter.Click += (s, e) => ChangerStatut("Rejete");

            flowBtns.Controls.Add(btnCreer);
            flowBtns.Controls.Add(btnApprouver);
            flowBtns.Controls.Add(btnDemarrer);
            flowBtns.Controls.Add(btnRejeter);
            panelForm.Controls.Add(flowBtns);

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
                lblTotal.Text = $"Total Ã  rembourser: {total:N2} USD";
            }
            else
            {
                lblMensualite.Text = "Mensualité: — USD";
                lblTotal.Text = "Total Ã  rembourser: — USD";
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





