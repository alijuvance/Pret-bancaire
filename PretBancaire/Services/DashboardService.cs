using PretBancaire.Data;

namespace PretBancaire.Services
{
    /// <summary>
    /// Service pour le tableau de bord — agrège les statistiques.
    /// </summary>
    public class DashboardService
    {
        private readonly ClientRepository _clientRepo = new();
        private readonly PretRepository _pretRepo = new();
        private readonly PaiementRepository _paiementRepo = new();

        /// <summary>
        /// Récupère toutes les statistiques du tableau de bord.
        /// </summary>
        public DashboardData GetStatistiques()
        {
            var stats = new DashboardData
            {
                NombreClients = _clientRepo.Count(),
                PretsParStatut = _pretRepo.CountByStatut(),
                MontantTotalEnCours = _pretRepo.GetMontantTotalEnCours(),
                TotalRemboursements = _paiementRepo.GetTotalRemboursements(),
                NombrePaiements = _paiementRepo.Count()
            };

            // Calculs dérivés
            stats.NombreTotalPrets = 0;
            foreach (var kvp in stats.PretsParStatut)
                stats.NombreTotalPrets += kvp.Value;

            return stats;
        }
    }

    /// <summary>
    /// Structure de données pour le tableau de bord.
    /// </summary>
    public class DashboardData
    {
        public int NombreClients { get; set; }
        public int NombreTotalPrets { get; set; }
        public Dictionary<string, int> PretsParStatut { get; set; } = new();
        public decimal MontantTotalEnCours { get; set; }
        public decimal TotalRemboursements { get; set; }
        public int NombrePaiements { get; set; }

        public int GetStatut(string statut) =>
            PretsParStatut.TryGetValue(statut, out int val) ? val : 0;
    }
}
