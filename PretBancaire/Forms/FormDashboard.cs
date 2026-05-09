using PretBancaire.Services;

namespace PretBancaire.Forms
{
    /// <summary>
    /// Tableau de bord — affiche les statistiques clés du système.
    /// </summary>
    public class FormDashboard : UserControl
    {
        public FormDashboard()
        {
            this.BackColor = Color.FromArgb(24, 28, 40);
            ChargerDonnees();
        }

        private void ChargerDonnees()
        {
            try
            {
                var service = new DashboardService();
                var data = service.GetStatistiques();
                ConstruireInterface(data);
            }
            catch (Exception ex)
            {
                var lbl = new Label
                {
                    Text = $"Erreur: {ex.Message}",
                    ForeColor = Color.Red,
                    Font = new Font("Segoe UI", 11),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                this.Controls.Add(lbl);
            }
        }

        private void ConstruireInterface(DashboardData data)
        {
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            // Cartes de statistiques
            flow.Controls.Add(CreerCarte("👥", "Clients", data.NombreClients.ToString(), Color.FromArgb(60, 130, 246)));
            flow.Controls.Add(CreerCarte("💰", "Total Prêts", data.NombreTotalPrets.ToString(), Color.FromArgb(139, 92, 246)));
            flow.Controls.Add(CreerCarte("⏳", "En Attente", data.GetStatut("EnAttente").ToString(), Color.FromArgb(245, 158, 11)));
            flow.Controls.Add(CreerCarte("✅", "Approuvés", data.GetStatut("Approuve").ToString(), Color.FromArgb(16, 185, 129)));
            flow.Controls.Add(CreerCarte("🔄", "En Cours", data.GetStatut("EnCours").ToString(), Color.FromArgb(59, 130, 246)));
            flow.Controls.Add(CreerCarte("🏁", "Terminés", data.GetStatut("Termine").ToString(), Color.FromArgb(34, 197, 94)));
            flow.Controls.Add(CreerCarte("❌", "Rejetés", data.GetStatut("Rejete").ToString(), Color.FromArgb(239, 68, 68)));
            flow.Controls.Add(CreerCarte("💵", "Montant En Cours", $"{data.MontantTotalEnCours:N2} USD", Color.FromArgb(168, 85, 247)));
            flow.Controls.Add(CreerCarte("💳", "Remboursements", $"{data.TotalRemboursements:N2} USD", Color.FromArgb(20, 184, 166)));
            flow.Controls.Add(CreerCarte("📝", "Nb Paiements", data.NombrePaiements.ToString(), Color.FromArgb(99, 102, 241)));

            this.Controls.Add(flow);
        }

        private Panel CreerCarte(string icon, string titre, string valeur, Color couleur)
        {
            var carte = new Panel
            {
                Size = new Size(250, 120),
                Margin = new Padding(8),
                BackColor = Color.FromArgb(32, 38, 55),
                Padding = new Padding(15)
            };

            // Bande de couleur à gauche
            var bande = new Panel
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = couleur
            };
            carte.Controls.Add(bande);

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 24),
                Location = new Point(20, 15),
                AutoSize = true
            };
            carte.Controls.Add(lblIcon);

            var lblTitre = new Label
            {
                Text = titre,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(150, 160, 180),
                Location = new Point(75, 18),
                AutoSize = true
            };
            carte.Controls.Add(lblTitre);

            var lblValeur = new Label
            {
                Text = valeur,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = couleur,
                Location = new Point(75, 45),
                AutoSize = true
            };
            carte.Controls.Add(lblValeur);

            return carte;
        }
    }
}
