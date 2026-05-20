using PretBancaire.Models;
using PretBancaire.Services;
using PretBancaire.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PretBancaire.Forms
{
    public class FormPaiements : UserControl
    {
        private readonly PaiementService _service = new();
        private readonly PretService _pretService = new();
        private DataGridView dgv = null!;
        private ComboBox cmbPret = null!, cmbModePaiement = null!;
        private TextBox txtMontant = null!, txtRef = null!;
        private DateTimePicker dtpDate = null!;
        private Button btnNouveau = null!, btnEnregistrer = null!, btnModifier = null!, btnSupprimer = null!;
        private int? _selectedId = null;

        public FormPaiements()
        {
            this.BackColor = UIHelper.BgDark;
            ConstruireInterface();
            ChargerDonnees();
        }

        private void ConstruireInterface()
        {
            var panelTop = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.Transparent, Padding = new Padding(15, 10, 15, 0) };
            var btnActualiser = UIHelper.CreerBouton("Actualiser", Color.FromArgb(100, 116, 139), 130);
            btnActualiser.Location = new Point(15, 10);
            btnActualiser.Click += (s, e) => ChargerDonnees();
            panelTop.Controls.Add(btnActualiser);
            this.Controls.Add(panelTop);

            dgv = new DataGridView();
            UIHelper.FormaterGrid(dgv);
            dgv.Dock = DockStyle.Fill;
            dgv.SelectionChanged += OnSelectionChanged;
            this.Controls.Add(dgv);

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

            cmbPret = new ComboBox { Font = new Font("Segoe UI", 10), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = UIHelper.BgInput, ForeColor = UIHelper.TextPrimary };
            cmbModePaiement = new ComboBox { Font = new Font("Segoe UI", 10), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = UIHelper.BgInput, ForeColor = UIHelper.TextPrimary };
            cmbModePaiement.Items.AddRange(new[] { "Especes", "Virement", "Cheque", "CarteBancaire" });
            cmbModePaiement.SelectedIndex = 0;

            txtMontant = UIHelper.CreerTextBox(130);
            txtRef = UIHelper.CreerTextBox(160);
            dtpDate = new DateTimePicker { Font = new Font("Segoe UI", 10), Format = DateTimePickerFormat.Short, Height = 34 };

            flowInputs.Controls.Add(UIHelper.CreerChamp("Pret en cours", cmbPret, 280));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Montant (Ar)", txtMontant, 130));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Date", dtpDate, 130));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Mode paiement", cmbModePaiement, 140));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Reference", txtRef, 160));

            panelForm.Controls.Add(flowInputs);

            // === 4 boutons CRUD ===
            var flowBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };

            btnNouveau = UIHelper.CreerBouton("Nouveau", Color.FromArgb(100, 116, 139), 120);
            btnNouveau.Click += (s, e) => Nouveau();

            btnEnregistrer = UIHelper.CreerBouton("Enregistrer", UIHelper.AccentGreen, 140);
            btnEnregistrer.Click += (s, e) => Sauvegarder();

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
                var paiements = _service.GetTousPaiements();
                dgv.DataSource = null;
                dgv.Columns.Clear();
                dgv.AutoGenerateColumns = true;
                dgv.DataSource = paiements;

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
                        case "NomClient": 
                            col.HeaderText = "Client"; 
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotPerson);
                            break;
                        case "Montant":
                            col.HeaderText = "Montant (Ar)";
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotMoney);
                            col.DefaultCellStyle.Format = "N2";
                            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                            break;
                        case "DatePaiement":
                            col.HeaderText = "Date Paiement";
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotDate);
                            col.DefaultCellStyle.Format = "dd/MM/yyyy";
                            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            break;
                        case "ModePaiement": 
                            col.HeaderText = "Mode"; 
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotSettings);
                            break;
                        case "Reference": 
                            col.HeaderText = "R\u00e9f\u00e9rence"; 
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotDocument);
                            break;
                        case "PretId":
                        case "ModePaiementLibelle":
                        case "Notes":
                            col.Visible = false;
                            break;
                    }
                }

                dgv.SelectionChanged += OnSelectionChanged;
                ChargerPretsEnCours();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur: " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChargerPretsEnCours()
        {
            cmbPret.Items.Clear();
            var prets = _pretService.GetPretsByStatut("EnCours");
            foreach (var p in prets)
            {
                decimal totalPaye = _service.GetTotalPaiements(p.Id);
                decimal restant = p.MontantTotal - totalPaye;
                cmbPret.Items.Add(new ComboItemPaiement
                {
                    Id = p.Id,
                    Text = p.NomClient + " - Reste: " + restant.ToString("N2") + " Ar"
                });
            }
        }

        private void OnSelectionChanged(object? sender, EventArgs e)
        {
            if (dgv.CurrentRow?.DataBoundItem is Paiement p)
            {
                _selectedId = p.Id;
                txtMontant.Text = p.Montant.ToString();
                dtpDate.Value = p.DatePaiement;
                txtRef.Text = p.Reference;

                // Sélectionner le mode de paiement
                int modeIdx = cmbModePaiement.Items.IndexOf(p.ModePaiement);
                if (modeIdx >= 0) cmbModePaiement.SelectedIndex = modeIdx;

                // Sélectionner le prêt correspondant
                for (int i = 0; i < cmbPret.Items.Count; i++)
                {
                    if (cmbPret.Items[i] is ComboItemPaiement item && item.Id == p.PretId)
                    {
                        cmbPret.SelectedIndex = i;
                        break;
                    }
                }

                btnModifier.Enabled = true;
                btnSupprimer.Enabled = true;
                btnEnregistrer.Enabled = false;
            }
        }

        private void Sauvegarder()
        {
            if (_selectedId.HasValue) return;

            var pretItem = cmbPret.SelectedItem as ComboItemPaiement;
            if (pretItem == null || pretItem.Id <= 0)
            {
                MessageBox.Show("Veuillez selectionner un pret valide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtMontant.Text, out decimal m) || m <= 0)
            {
                MessageBox.Show("Veuillez entrer un montant valide superieur a 0.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var paiement = new Paiement
                {
                    PretId = pretItem.Id,
                    Montant = m,
                    DatePaiement = dtpDate.Value,
                    ModePaiement = cmbModePaiement.SelectedItem?.ToString() ?? "",
                    Reference = txtRef.Text.Trim()
                };

                var (success, message) = _service.EnregistrerPaiement(paiement);

                MessageBox.Show(message, success ? "Succes" : "Erreur",
                    MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                if (success) { ChargerDonnees(); Nouveau(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur: " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Modifier()
        {
            if (!_selectedId.HasValue) return;

            if (!decimal.TryParse(txtMontant.Text, out decimal m) || m <= 0)
            {
                MessageBox.Show("Veuillez entrer un montant valide superieur a 0.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var paiement = new Paiement
                {
                    Id = _selectedId.Value,
                    Montant = m,
                    DatePaiement = dtpDate.Value,
                    ModePaiement = cmbModePaiement.SelectedItem?.ToString() ?? "",
                    Reference = txtRef.Text.Trim()
                };

                var (success, message) = _service.ModifierPaiement(paiement);

                MessageBox.Show(message, success ? "Succès" : "Erreur",
                    MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                if (success) { ChargerDonnees(); Nouveau(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur: " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Supprimer()
        {
            if (!_selectedId.HasValue)
            {
                MessageBox.Show("Selectionnez un paiement.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Voulez-vous vraiment supprimer ce paiement ?", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            var (success, message) = _service.SupprimerPaiement(_selectedId.Value);
            MessageBox.Show(message, success ? "Succès" : "Erreur",
                MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (success) { ChargerDonnees(); Nouveau(); }
        }

        private void Nouveau()
        {
            _selectedId = null;
            if (cmbPret.Items.Count > 0) cmbPret.SelectedIndex = -1;
            txtMontant.Clear();
            txtRef.Clear();
            cmbModePaiement.SelectedIndex = 0;
            dtpDate.Value = DateTime.Today;
            btnEnregistrer.Enabled = true;
            btnModifier.Enabled = false;
            btnSupprimer.Enabled = false;
            dgv.ClearSelection();
        }
    }

    class ComboItemPaiement
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public override string ToString() => Text;
    }
}