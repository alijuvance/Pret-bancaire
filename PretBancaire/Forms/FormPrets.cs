using PretBancaire.Models;
using PretBancaire.Services;
using PretBancaire.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PretBancaire.Forms
{
    public class FormPrets : UserControl
    {
        private readonly PretService _service = new();
        private readonly ClientService _clientService = new();
        private DataGridView dgv = null!;
        private ComboBox cmbClient = null!, cmbStatutFiltre = null!;
        private TextBox txtMontant = null!, txtTaux = null!, txtDuree = null!, txtNotes = null!;
        private Label lblMensualite = null!, lblTotal = null!;
        private int? _selectedId = null;

        public FormPrets()
        {
            this.BackColor = UIHelper.BgDark;
            ConstruireInterface();
            ChargerDonnees();
        }

        private void ConstruireInterface()
        {
            // === Barre du haut : filtre par statut ===
            var panelTop = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.Transparent, Padding = new Padding(15, 12, 15, 0) };

            var lblFiltre = new Label { Text = "FILTRE :", ForeColor = UIHelper.TextSecondary, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), AutoSize = true, Location = new Point(15, 18) };
            cmbStatutFiltre = new ComboBox { Location = new Point(80, 13), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), BackColor = UIHelper.BgInput, ForeColor = UIHelper.TextPrimary };
            cmbStatutFiltre.Items.AddRange(new[] { "Tous", "EnAttente", "Approuve", "Rejete", "EnCours", "Termine" });
            cmbStatutFiltre.SelectedIndex = 0;
            cmbStatutFiltre.SelectedIndexChanged += (s, e) => Filtrer();

            panelTop.Controls.Add(lblFiltre);
            panelTop.Controls.Add(cmbStatutFiltre);
            this.Controls.Add(panelTop);

            // === DataGridView ===
            dgv = new DataGridView();
            UIHelper.FormaterGrid(dgv);
            dgv.Dock = DockStyle.Fill;
            dgv.SelectionChanged += OnSelectionChanged;
            this.Controls.Add(dgv);

            // === Panel formulaire en bas ===
            var panelForm = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 170,
                BackColor = UIHelper.BgCard,
                Padding = new Padding(20, 12, 20, 12)
            };

            // Ligne 1 : Champs de saisie
            var flowInputs = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 72,
                WrapContents = true,
                Padding = new Padding(0)
            };

            cmbClient = new ComboBox { Font = new Font("Segoe UI", 10), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = UIHelper.BgInput, ForeColor = UIHelper.TextPrimary };
            foreach (var c in _clientService.GetTousClients())
                cmbClient.Items.Add(new ComboItem { Id = c.Id, Text = c.Nom + " " + c.Prenom + " (" + c.Cin + ")" });

            txtMontant = UIHelper.CreerTextBox(130); txtMontant.TextChanged += (s, e) => CalculerApercu();
            txtTaux = UIHelper.CreerTextBox(80); txtTaux.Text = "8.5"; txtTaux.TextChanged += (s, e) => CalculerApercu();
            txtDuree = UIHelper.CreerTextBox(80); txtDuree.Text = "12"; txtDuree.TextChanged += (s, e) => CalculerApercu();
            txtNotes = UIHelper.CreerTextBox(200);

            flowInputs.Controls.Add(UIHelper.CreerChamp("Client", cmbClient, 220));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Montant (USD)", txtMontant, 130));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Taux %", txtTaux, 80));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Duree (Mois)", txtDuree, 80));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Notes", txtNotes, 200));

            // Apercu mensualite
            var panelApercu = new Panel { Width = 240, Height = 62, Margin = new Padding(15, 0, 0, 0) };
            lblMensualite = new Label { Text = "Mensualite: 0.00 USD", Dock = DockStyle.Top, Height = 28, ForeColor = UIHelper.AccentBlue, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            lblTotal = new Label { Text = "Total a rembourser: 0.00 USD", Dock = DockStyle.Bottom, Height = 22, ForeColor = UIHelper.TextSecondary, Font = new Font("Segoe UI", 9) };
            panelApercu.Controls.Add(lblMensualite);
            panelApercu.Controls.Add(lblTotal);
            flowInputs.Controls.Add(panelApercu);

            panelForm.Controls.Add(flowInputs);

            // Ligne 2 : 4 boutons principaux
            var flowBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };

            var btnNouveau = UIHelper.CreerBouton("Nouveau", Color.FromArgb(100, 116, 139), 120);
            btnNouveau.Click += (s, e) => Nouveau();

            var btnEnregistrer = UIHelper.CreerBouton("Enregistrer", UIHelper.AccentBlue, 140);
            btnEnregistrer.Click += (s, e) => CreerPret();

            var btnModifier = UIHelper.CreerBouton("Modifier", UIHelper.AccentGreen, 130);
            btnModifier.Click += (s, e) => ModifierStatut();

            var btnRejeter = UIHelper.CreerBouton("Rejeter", UIHelper.AccentRed, 120);
            btnRejeter.Click += (s, e) => ChangerStatut("Rejete");

            flowBtns.Controls.Add(btnNouveau);
            flowBtns.Controls.Add(btnEnregistrer);
            flowBtns.Controls.Add(btnModifier);
            flowBtns.Controls.Add(btnRejeter);

            panelForm.Controls.Add(flowBtns);
            this.Controls.Add(panelForm);
        }

        private void ChargerDonnees()
        {
            try
            {
                dgv.SelectionChanged -= OnSelectionChanged;
                dgv.DataSource = null;
                dgv.Columns.Clear();
                dgv.AutoGenerateColumns = true;
                dgv.DataSource = _service.GetTousPrets();

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
                        case "TauxInteret":
                            col.HeaderText = "Taux %";
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotSettings);
                            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            break;
                        case "DureeMois":
                            col.HeaderText = "Dur\u00e9e";
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotDate);
                            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            break;
                        case "Mensualite":
                            col.HeaderText = "Mensualit\u00e9";
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotMoney);
                            col.DefaultCellStyle.Format = "N2";
                            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                            break;
                        case "MontantTotal":
                            col.HeaderText = "Total (USD)";
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotMoney);
                            col.DefaultCellStyle.Format = "N2";
                            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                            break;
                        case "StatutLibelle": 
                            col.HeaderText = "Statut"; 
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotStatus);
                            break;
                        case "DateDemande":
                            col.HeaderText = "Date Demande";
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotDate);
                            col.DefaultCellStyle.Format = "dd/MM/yyyy";
                            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            break;
                        case "ClientId":
                        case "Statut":
                        case "DateApprobation":
                        case "Notes":
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
                lblMensualite.Text = "Mensualite: " + mensualite.ToString("N2") + " USD";
                lblTotal.Text = "Total a rembourser: " + total.ToString("N2") + " USD";
            }
            else
            {
                lblMensualite.Text = "Mensualite: -- USD";
                lblTotal.Text = "Total a rembourser: -- USD";
            }
        }

        private void OnSelectionChanged(object? sender, EventArgs e)
        {
            if (dgv.CurrentRow?.DataBoundItem is Pret p)
                _selectedId = p.Id;
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

                MessageBox.Show(message, success ? "Succes" : "Erreur",
                    MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                if (success) { ChargerDonnees(); Nouveau(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur: " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ModifierStatut()
        {
            if (!_selectedId.HasValue)
            {
                MessageBox.Show("Veuillez selectionner un pret.", "Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var choix = new string[] { "Approuve", "EnCours", "Termine" };
            var form = new Form
            {
                Text = "Modifier le statut",
                Size = new Size(320, 200),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = UIHelper.BgDark,
                ForeColor = UIHelper.TextPrimary,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lbl = new Label { Text = "Nouveau statut :", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10) };
            var cmb = new ComboBox { Location = new Point(20, 50), Width = 260, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cmb.Items.AddRange(choix);
            cmb.SelectedIndex = 0;

            var btnOk = UIHelper.CreerBouton("Valider", UIHelper.AccentGreen, 120);
            btnOk.Location = new Point(85, 110);
            btnOk.Click += (s, e) => { form.DialogResult = DialogResult.OK; form.Close(); };

            form.Controls.AddRange(new Control[] { lbl, cmb, btnOk });

            if (form.ShowDialog() == DialogResult.OK)
            {
                ChangerStatut(cmb.SelectedItem?.ToString() ?? "Approuve");
            }
        }

        private void ChangerStatut(string nouveauStatut)
        {
            if (!_selectedId.HasValue)
            {
                MessageBox.Show("Veuillez selectionner un pret.", "Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var (success, message) = _service.ChangerStatutPret(_selectedId.Value, nouveauStatut);
                MessageBox.Show(message, success ? "Succes" : "Erreur",
                    MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                if (success) ChargerDonnees();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur: " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Nouveau()
        {
            _selectedId = null;
            txtMontant.Clear(); txtTaux.Text = "8.5"; txtDuree.Text = "12"; txtNotes.Clear();
            if (cmbClient.Items.Count > 0) cmbClient.SelectedIndex = -1;
            dgv.ClearSelection();
        }
    }

    class ComboItem
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public override string ToString() => Text;
    }
}