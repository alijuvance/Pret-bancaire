using PretBancaire.Models;
using PretBancaire.Services;

namespace PretBancaire.Forms
{
    /// <summary>
    /// Formulaire de gestion des paiements (remboursements).
    /// </summary>
    public class FormPaiements : UserControl
    {
        private readonly PaiementService _service = new();
        private readonly PretService _pretService = new();
        private DataGridView dgvPrets = null!, dgvPaiements = null!;
        private ComboBox cmbMode = null!;
        private TextBox txtMontant = null!, txtRef = null!, txtNotes = null!;
        private DateTimePicker dtpDate = null!;
        private Label lblRestant = null!, lblInfoPret = null!;
        private int? _selectedPretId = null;

        public FormPaiements()
        {
            this.BackColor = Color.FromArgb(24, 28, 40);
            ConstruireInterface();
            ChargerPrets();
        }

        private void ConstruireInterface()
        {
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 250,
                BackColor = Color.FromArgb(24, 28, 40),
                Panel1MinSize = 150,
                Panel2MinSize = 150
            };
            this.Controls.Add(splitContainer);

            // === Panel haut : Liste des prêts en cours ===
            var lblTitrePrets = new Label
            {
                Text = "📋 Prêts en cours (sélectionnez un prêt)",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 180, 255),
                Dock = DockStyle.Top, Height = 30
            };
            splitContainer.Panel1.Controls.Add(lblTitrePrets);

            dgvPrets = CreerGridView();
            dgvPrets.Dock = DockStyle.Fill;
            dgvPrets.SelectionChanged += (s, e) => PretSelectionne();
            splitContainer.Panel1.Controls.Add(dgvPrets);

            // === Panel bas : Paiements + formulaire ===
            lblInfoPret = new Label
            {
                Text = "Sélectionnez un prêt ci-dessus pour voir ses paiements",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(150, 160, 180),
                Dock = DockStyle.Top, Height = 25
            };
            splitContainer.Panel2.Controls.Add(lblInfoPret);

            dgvPaiements = CreerGridView();
            dgvPaiements.Dock = DockStyle.Fill;
            splitContainer.Panel2.Controls.Add(dgvPaiements);

            // === Panel formulaire paiement ===
            var panelForm = new Panel
            {
                Dock = DockStyle.Bottom, Height = 100,
                BackColor = Color.FromArgb(28, 32, 46), Padding = new Padding(15)
            };

            int x = 15, y = 10;
            panelForm.Controls.Add(Lbl("Montant:", x, y));
            txtMontant = Txt(x + 75, y, 120); panelForm.Controls.Add(txtMontant);

            panelForm.Controls.Add(Lbl("Date:", x + 210, y));
            dtpDate = new DateTimePicker
            {
                Location = new Point(x + 255, y), Size = new Size(130, 28),
                Font = new Font("Segoe UI", 10), Format = DateTimePickerFormat.Short
            };
            panelForm.Controls.Add(dtpDate);

            panelForm.Controls.Add(Lbl("Mode:", x + 400, y));
            cmbMode = new ComboBox
            {
                Font = new Font("Segoe UI", 10), Size = new Size(140, 28),
                Location = new Point(x + 450, y), DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 52, 70), ForeColor = Color.White
            };
            cmbMode.Items.AddRange(new[] { "Especes", "Virement", "Cheque", "CarteBancaire" });
            cmbMode.SelectedIndex = 0;
            panelForm.Controls.Add(cmbMode);

            y = 50;
            panelForm.Controls.Add(Lbl("Réf:", x, y));
            txtRef = Txt(x + 40, y, 120); panelForm.Controls.Add(txtRef);

            panelForm.Controls.Add(Lbl("Notes:", x + 175, y));
            txtNotes = Txt(x + 230, y, 200); panelForm.Controls.Add(txtNotes);

            lblRestant = new Label
            {
                Text = "Restant: — USD", Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(245, 158, 11), Location = new Point(x + 450, y + 3), AutoSize = true
            };
            panelForm.Controls.Add(lblRestant);

            var btnPayer = new Button
            {
                Text = "💳 Enregistrer le paiement",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(200, 38), Location = new Point(x + 630, y - 20),
                BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnPayer.FlatAppearance.BorderSize = 0;
            btnPayer.Click += (s, e) => EnregistrerPaiement();
            panelForm.Controls.Add(btnPayer);

            splitContainer.Panel2.Controls.Add(panelForm);
        }

        private void ChargerPrets()
        {
            try
            {
                var prets = _pretService.GetPretsByStatut("EnCours");
                dgvPrets.DataSource = null;
                dgvPrets.Columns.Clear();
                dgvPrets.AutoGenerateColumns = true;
                dgvPrets.DataSource = prets;

                foreach (DataGridViewColumn col in dgvPrets.Columns)
                {
                    switch (col.Name)
                    {
                        case "Id": col.HeaderText = "ID"; col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; col.Width = 50; break;
                        case "NomClient": col.HeaderText = "Client"; break;
                        case "Montant": col.HeaderText = "Montant"; col.DefaultCellStyle.Format = "N2"; break;
                        case "Mensualite": col.HeaderText = "Mensualité"; col.DefaultCellStyle.Format = "N2"; break;
                        case "MontantTotal": col.HeaderText = "Total"; col.DefaultCellStyle.Format = "N2"; break;
                        case "DureeMois": col.HeaderText = "Durée"; break;
                        default: col.Visible = false; break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.StackTrace}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PretSelectionne()
        {
            try
            {
                if (dgvPrets.CurrentRow?.DataBoundItem is not Pret pret) return;

                _selectedPretId = pret.Id;
                var restant = _pretService.GetMontantRestant(pret.Id);
                lblRestant.Text = $"Restant: {restant:N2} USD";
                lblInfoPret.Text = $"Paiements pour le prêt #{pret.Id} — {pret.NomClient}";

                var paiements = _service.GetPaiementsByPret(pret.Id);
                dgvPaiements.DataSource = null;
                dgvPaiements.Columns.Clear();
                dgvPaiements.AutoGenerateColumns = true;
                dgvPaiements.DataSource = paiements;

                foreach (DataGridViewColumn col in dgvPaiements.Columns)
                {
                    switch (col.Name)
                    {
                        case "Id": col.HeaderText = "ID"; col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; col.Width = 50; break;
                        case "Montant": col.HeaderText = "Montant"; col.DefaultCellStyle.Format = "N2"; break;
                        case "DatePaiement": col.HeaderText = "Date"; col.DefaultCellStyle.Format = "dd/MM/yyyy"; break;
                        case "ModePaiementLibelle": col.HeaderText = "Mode"; break;
                        case "Reference": col.HeaderText = "Référence"; break;
                        default: col.Visible = false; break;
                    }
                }
            }
            catch { }
        }

        private void EnregistrerPaiement()
        {
            if (!_selectedPretId.HasValue)
            {
                MessageBox.Show("Veuillez sélectionner un prêt.", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtMontant.Text, out decimal montant) || montant <= 0)
            {
                MessageBox.Show("Montant invalide.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var paiement = new Paiement
                {
                    PretId = _selectedPretId.Value,
                    Montant = montant,
                    DatePaiement = dtpDate.Value,
                    ModePaiement = cmbMode.SelectedItem?.ToString() ?? "Especes",
                    Reference = txtRef.Text.Trim(),
                    Notes = txtNotes.Text.Trim()
                };

                var (success, message) = _service.EnregistrerPaiement(paiement);
                MessageBox.Show(message, success ? "Succès" : "Erreur",
                    MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                if (success)
                {
                    txtMontant.Clear(); txtRef.Clear(); txtNotes.Clear();
                    PretSelectionne(); // Rafraîchir
                    ChargerPrets();    // Rafraîchir la liste si prêt terminé
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.StackTrace}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // === Helpers ===
        private static DataGridView CreerGridView()
        {
            var g = new DataGridView
            {
                BackgroundColor = Color.FromArgb(28, 32, 46), ForeColor = Color.White,
                GridColor = Color.FromArgb(50, 55, 70), BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10)
            };
            g.DefaultCellStyle.BackColor = Color.FromArgb(32, 38, 55);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 130, 246);
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(16, 20, 32);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(150, 160, 180);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersHeight = 38;
            g.RowTemplate.Height = 32;
            return g;
        }

        private static Label Lbl(string t, int x, int y) => new()
        { Text = t, Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(200, 210, 225), Location = new Point(x, y + 3), AutoSize = true };

        private static TextBox Txt(int x, int y, int w) => new()
        { Font = new Font("Segoe UI", 10), Size = new Size(w, 28), Location = new Point(x, y), BackColor = Color.FromArgb(45, 52, 70), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
    }
}


