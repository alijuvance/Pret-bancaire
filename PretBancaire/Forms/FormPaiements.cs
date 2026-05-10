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
            cmbModePaiement.Items.AddRange(new[] { "Especes", "Virement", "Carte", "Mobile Money" });
            cmbModePaiement.SelectedIndex = 0;

            txtMontant = UIHelper.CreerTextBox(130);
            txtRef = UIHelper.CreerTextBox(160);
            dtpDate = new DateTimePicker { Font = new Font("Segoe UI", 10), Format = DateTimePickerFormat.Short, Height = 34 };

            flowInputs.Controls.Add(UIHelper.CreerChamp("Pret en cours", cmbPret, 280));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Montant (USD)", txtMontant, 130));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Date", dtpDate, 130));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Mode paiement", cmbModePaiement, 140));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Reference", txtRef, 160));

            panelForm.Controls.Add(flowInputs);

            var flowBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };

            var btnNouveau = UIHelper.CreerBouton("Nouveau", Color.FromArgb(100, 116, 139), 120);
            btnNouveau.Click += (s, e) => Nouveau();

            var btnEnregistrer = UIHelper.CreerBouton("Enregistrer", UIHelper.AccentGreen, 140);
            btnEnregistrer.Click += (s, e) => Sauvegarder();

            flowBtns.Controls.Add(btnNouveau);
            flowBtns.Controls.Add(btnEnregistrer);

            panelForm.Controls.Add(flowBtns);
            this.Controls.Add(panelForm);
        }

        private void ChargerDonnees()
        {
            try
            {
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
                            col.HeaderText = "Montant (USD)";
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
                        case "Notes":
                            col.Visible = false;
                            break;
                    }
                }

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
                cmbPret.Items.Add(new ComboItemPaiement
                {
                    Id = p.Id,
                    Text = p.NomClient + " - Reste: " + _service.GetTotalPaiements(p.Id).ToString("N2") + " USD"
                });
            }
        }

        private void Sauvegarder()
        {
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

        private void Nouveau()
        {
            if (cmbPret.Items.Count > 0) cmbPret.SelectedIndex = -1;
            txtMontant.Clear();
            txtRef.Clear();
            cmbModePaiement.SelectedIndex = 0;
            dtpDate.Value = DateTime.Today;
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