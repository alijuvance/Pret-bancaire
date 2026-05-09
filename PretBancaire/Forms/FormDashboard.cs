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
            this.BackColor = Color.FromArgb(0, 0, 0);
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
            var mainFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(0, 0, 0) // slate-900
            };

            // Helpers pour les couleurs
            var cPrimary = Color.FromArgb(0, 112, 243);  // blue-500
            var cSuccess = Color.FromArgb(16, 185, 129); // emerald-500
            var cWarning = Color.FromArgb(245, 158, 11); // amber-500
            var cDanger = Color.FromArgb(239, 68, 68);   // red-500
            var cPurple = Color.FromArgb(168, 85, 247);  // purple-500
            var cTeal = Color.FromArgb(20, 184, 166);    // teal-500

            // Ligne 1 : Global
            mainFlow.Controls.Add(CreerCarte("👥", "Total Clients", data.NombreClients.ToString(), cPrimary));
            mainFlow.Controls.Add(CreerCarte("💰", "Prêts Accordés", data.NombreTotalPrets.ToString(), cPurple));
            mainFlow.Controls.Add(CreerCarte("💵", "Montant En Cours", $"{data.MontantTotalEnCours:N2} USD", cPrimary, 350));
            mainFlow.Controls.Add(CreerCarte("💳", "Total Remboursé", $"{data.TotalRemboursements:N2} USD", cTeal, 350));

            // Ligne 2 : Statuts
            mainFlow.Controls.Add(CreerCarte("⏳", "En Attente", data.GetStatut("EnAttente").ToString(), cWarning));
            mainFlow.Controls.Add(CreerCarte("✅", "Approuvés", data.GetStatut("Approuve").ToString(), cSuccess));
            mainFlow.Controls.Add(CreerCarte("🔄", "En Cours", data.GetStatut("EnCours").ToString(), cPrimary));
            mainFlow.Controls.Add(CreerCarte("🏁", "Terminés", data.GetStatut("Termine").ToString(), cSuccess));
            mainFlow.Controls.Add(CreerCarte("❌", "Rejetés", data.GetStatut("Rejete").ToString(), cDanger));

            this.Controls.Add(mainFlow);
        }

        private Panel CreerCarte(string icon, string titre, string valeur, Color couleur, int width = 280)
        {
            var carte = new Panel
            {
                Size = new Size(width, 130),
                Margin = new Padding(15),
                BackColor = Color.FromArgb(10, 10, 10), // slate-800
                Padding = new Padding(0)
            };

            // Bordure colorée en haut (design moderne)
            var bordure = new Panel { Dock = DockStyle.Top, Height = 4, BackColor = couleur };
            carte.Controls.Add(bordure);

            // Container interne
            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(15)
            };
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 60));

            // Icône
            var lblIcon = new Label
            {
                Text = icon, Font = new Font("Segoe UI", 28),
                ForeColor = couleur, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            container.Controls.Add(lblIcon, 0, 0);
            container.SetRowSpan(lblIcon, 2);

            // Titre
            var lblTitre = new Label
            {
                Text = titre.ToUpper(), Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(161, 161, 170), // slate-400
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft
            };
            container.Controls.Add(lblTitre, 1, 0);

            // Valeur
            var lblValeur = new Label
            {
                Text = valeur, Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(0, 5, 0, 0)
            };
            container.Controls.Add(lblValeur, 1, 1);

            carte.Controls.Add(container);
            return carte;
        }
    }

}
