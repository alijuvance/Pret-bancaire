using PretBancaire.Models;
using PretBancaire.Services;
using PretBancaire.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PretBancaire.Forms
{
    public class FormUtilisateurs : UserControl
    {
        private readonly AuthService _service = new();
        private DataGridView dgv = null!;
        private TextBox txtLogin = null!, txtNom = null!, txtPrenom = null!, txtMdp = null!;
        private Button btnNouveau = null!, btnEnregistrer = null!, btnModifier = null!, btnSupprimer = null!;
        private CheckBox chkActif = null!;
        private int? _selectedId = null;

        public FormUtilisateurs()
        {
            this.BackColor = UIHelper.BgDark;
            ConstruireInterface();
            ChargerDonnees();
        }

        private void ConstruireInterface()
        {
            var panelTop = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.Transparent, Padding = new Padding(15, 10, 15, 0) };
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

            txtNom = UIHelper.CreerTextBox(150);
            txtPrenom = UIHelper.CreerTextBox(150);
            txtLogin = UIHelper.CreerTextBox(150);
            txtMdp = UIHelper.CreerTextBox(150);
            txtMdp.PasswordChar = (char)8226;



            chkActif = new CheckBox
            {
                Text = "Actif",
                ForeColor = UIHelper.TextPrimary,
                Font = new Font("Segoe UI", 10),
                Checked = true,
                AutoSize = true,
                Margin = new Padding(10, 30, 0, 0)
            };

            flowInputs.Controls.Add(UIHelper.CreerChamp("Nom", txtNom, 150));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Prenom", txtPrenom, 150));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Login", txtLogin, 150));
            flowInputs.Controls.Add(UIHelper.CreerChamp("Mot de passe", txtMdp, 150));
            flowInputs.Controls.Add(chkActif);

            panelForm.Controls.Add(flowInputs);

            var flowBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };

            btnNouveau = UIHelper.CreerBouton("Nouveau", Color.FromArgb(100, 116, 139), 120);
            btnNouveau.Click += (s, e) => Nouveau();

            btnEnregistrer = UIHelper.CreerBouton("Enregistrer", UIHelper.AccentBlue, 120);
            btnEnregistrer.Click += (s, e) => Ajouter();

            btnModifier = UIHelper.CreerBouton("Modifier", UIHelper.AccentOrange, 120);
            btnModifier.Click += (s, e) => Modifier();
            btnModifier.Enabled = false;

            btnSupprimer = UIHelper.CreerBouton("Supprimer", UIHelper.AccentRed, 120);
            btnSupprimer.Click += (s, e) => SupprimerUtilisateur();
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
                var users = _service.GetTousUtilisateurs();
                dgv.DataSource = null;
                dgv.Columns.Clear();
                dgv.AutoGenerateColumns = true;
                dgv.DataSource = users;

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
                        case "Login": 
                            col.HeaderText = "Identifiant"; 
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotSecurity);
                            break;
                        case "Role": 
                            col.Visible = false;
                            break;
                        case "Actif": 
                            col.HeaderText = "Actif"; 
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotStatus);
                            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; 
                            break;
                        case "DateCreation":
                            col.HeaderText = "Date Cr\u00e9ation";
                            UIHelper.SetColumnDotColor(dgv, col.Name, UIHelper.DotDate);
                            col.DefaultCellStyle.Format = "dd/MM/yyyy";
                            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            break;
                        case "MotDePasse":
                        case "NomComplet":
                        case "EstAdmin":
                            col.Visible = false;
                            break;
                    }
                }

                dgv.SelectionChanged += OnSelectionChanged;
            }
            catch { }
        }

        private void OnSelectionChanged(object? sender, EventArgs e)
        {
            if (dgv.CurrentRow?.DataBoundItem is Utilisateur u)
            {
                _selectedId = u.Id;
                txtNom.Text = u.Nom;
                txtPrenom.Text = u.Prenom;
                txtLogin.Text = u.Login;
                txtMdp.Clear();
                chkActif.Checked = u.Actif;
                if (btnModifier != null) btnModifier.Enabled = true;
                if (btnSupprimer != null) btnSupprimer.Enabled = true;
                if (btnEnregistrer != null) btnEnregistrer.Enabled = false;
            }
        }

        private void Ajouter()
        {
            if (_selectedId.HasValue) return;

            if (string.IsNullOrWhiteSpace(txtLogin.Text) || string.IsNullOrWhiteSpace(txtNom.Text))
            {
                MessageBox.Show("Veuillez remplir les champs obligatoires (Nom, Login).", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMdp.Text))
            {
                MessageBox.Show("Le mot de passe est obligatoire pour un nouvel utilisateur.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var u = new Utilisateur
                {
                    Nom = txtNom.Text.Trim(),
                    Prenom = txtPrenom.Text.Trim(),
                    Login = txtLogin.Text.Trim(),
                    Actif = chkActif.Checked,
                    MotDePasse = txtMdp.Text.Trim()
                };

                if (_service.LoginExiste(u.Login, null))
                {
                    MessageBox.Show("Ce login est déjà utilisé.", "Doublon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _service.AjouterUtilisateur(u);
                ChargerDonnees();
                Nouveau();
                MessageBox.Show("Utilisateur enregistré avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur: " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Modifier()
        {
            if (!_selectedId.HasValue) return;

            if (string.IsNullOrWhiteSpace(txtLogin.Text) || string.IsNullOrWhiteSpace(txtNom.Text))
            {
                MessageBox.Show("Veuillez remplir les champs obligatoires (Nom, Login).", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var u = new Utilisateur
                {
                    Id = _selectedId.Value,
                    Nom = txtNom.Text.Trim(),
                    Prenom = txtPrenom.Text.Trim(),
                    Login = txtLogin.Text.Trim(),
                    Actif = chkActif.Checked
                };

                if (_service.LoginExiste(u.Login, _selectedId))
                {
                    MessageBox.Show("Ce login est déjà utilisé.", "Doublon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (u.Id == AuthService.UtilisateurConnecte?.Id && !u.Actif)
                {
                    MessageBox.Show("Vous ne pouvez pas désactiver votre propre compte.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _service.ModifierUtilisateur(u);
                
                string newMdp = txtMdp.Text.Trim();
                if (!string.IsNullOrWhiteSpace(newMdp))
                    _service.ModifierMotDePasse(u.Id, newMdp);

                ChargerDonnees();
                Nouveau();
                MessageBox.Show("Utilisateur modifié avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur: " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SupprimerUtilisateur()
        {
            if (!_selectedId.HasValue) { MessageBox.Show("Selectionnez un utilisateur.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (_selectedId.Value == AuthService.UtilisateurConnecte?.Id)
            {
                MessageBox.Show("Vous ne pouvez pas supprimer votre propre compte.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("Voulez-vous vraiment supprimer cet utilisateur ?", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            _service.SupprimerUtilisateur(_selectedId.Value);
            ChargerDonnees();
            Nouveau();
            MessageBox.Show("Utilisateur supprime.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Nouveau()
        {
            _selectedId = null;
            txtNom.Clear(); txtPrenom.Clear(); txtLogin.Clear(); txtMdp.Clear();
            chkActif.Checked = true;
            if (btnEnregistrer != null) btnEnregistrer.Enabled = true;
            if (btnModifier != null) btnModifier.Enabled = false;
            if (btnSupprimer != null) btnSupprimer.Enabled = false;
            dgv.ClearSelection();
        }
    }
}