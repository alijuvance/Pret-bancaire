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
            this.BackColor = Color.FromArgb(0, 0, 0);
            ConstruireInterface();
            ChargerPrets();
        }

        private void ConstruireInterface()
        {
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill, Orientation = Orientation.Horizontal,
                SplitterDistance = 250, BackColor = Color.FromArgb(0, 0, 0),
                Panel1MinSize = 150, Panel2MinSize = 150
            };
            this.Controls.Add(splitContainer);

            // === Panel haut : Liste des Prêts en cours ===
            var lblTitrePrets = new Label
            {
                Text = "📋 PRÊTS EN COURS (SÉLECTIONNEZ UN PRÃŠT)",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(161, 161, 170),
                Dock = DockStyle.Top, Height = 35, Padding = new Padding(10, 10, 0, 0)
            };
            splitContainer.Panel1.Controls.Add(lblTitrePrets);

            dgvPrets = CreerGridView();
            dgvPrets.Dock = DockStyle.Fill;
            dgvPrets.SelectionChanged += (s, e) => PretSelectionne();
            splitContainer.Panel1.Controls.Add(dgvPrets);

            // === Panel bas : Paiements + formulaire ===
            lblInfoPret = new Label
            {
                Text = "SÉLECTIONNEZ UN PRÃŠT CI-DESSUS POUR VOIR SES PAIEMENTS",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(161, 161, 170),
                Dock = DockStyle.Top, Height = 35, Padding = new Padding(10, 10, 0, 0)
            };
            splitContainer.Panel2.Controls.Add(lblInfoPret);

            dgvPaiements = CreerGridView();
            dgvPaiements.Dock = DockStyle.Fill;
            splitContainer.Panel2.Controls.Add(dgvPaiements);

            // === Panel formulaire paiement ===
            var panelForm = new Panel
            {
                Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.FromArgb(10, 10, 10), Padding = new Padding(20)
            };

            var flowInputs = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(0, 0, 0, 10)
            };

            txtMontant = CreerInputText();
            dtpDate = new DateTimePicker { Font = new Font("Segoe UI", 10), Format = DateTimePickerFormat.Short, Height = 35 };
            
            cmbMode = new ComboBox
            {
                Font = new Font("Segoe UI", 10), Height = 35,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(20, 20, 20), ForeColor = Color.White
            };
            cmbMode.Items.AddRange(new[] { "Especes", "Virement", "Cheque", "CarteBancaire" });
            cmbMode.SelectedIndex = 0;

            txtRef = CreerInputText();
            txtNotes = CreerInputText();

            flowInputs.Controls.Add(CreerChamp("Montant", txtMontant, 150));
            flowInputs.Controls.Add(CreerChamp("Date", dtpDate, 150));
            flowInputs.Controls.Add(CreerChamp("Mode", cmbMode, 150));
            flowInputs.Controls.Add(CreerChamp("Réf", txtRef, 150));
            flowInputs.Controls.Add(CreerChamp("Notes", txtNotes, 300));
            
            lblRestant = new Label
            {
                Text = "Restant: — USD", Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(245, 158, 11), AutoSize = true,
                Margin = new Padding(20, 20, 0, 0)
            };
            flowInputs.Controls.Add(lblRestant);
            
            panelForm.Controls.Add(flowInputs);

            var flowBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 60, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 15, 0, 0)
            };

            var btnPayer = new Button
            {
                Text = "Enregistrer le paiement", Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(220, 40), Margin = new Padding(10, 0, 0, 0),
                BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnPayer.FlatAppearance.BorderSize = 0;
            btnPayer.Click += (s, e) => EnregistrerPaiement();
            
            flowBtns.Controls.Add(btnPayer);
            panelForm.Controls.Add(flowBtns);

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
                lblInfoPret.Text = $"Paiements pour le Prêt #{pret.Id} — {pret.NomClient}";

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
                MessageBox.Show("Veuillez sélectionner un Prêt.", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    ChargerPrets();    // Rafraîchir la liste si Prêt terminé
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
                BackgroundColor = Color.FromArgb(0, 0, 0), ForeColor = Color.White,
                GridColor = Color.FromArgb(20, 20, 20), BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10)
            };
            g.DefaultCellStyle.BackColor = Color.FromArgb(10, 10, 10);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 112, 243);
            g.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 0, 0);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(161, 161, 170);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersHeight = 50;
            g.RowTemplate.Height = 45;
            return g;
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
    }
}





