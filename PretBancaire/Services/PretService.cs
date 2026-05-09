using PretBancaire.Data;
using PretBancaire.Models;

namespace PretBancaire.Services
{
    /// <summary>
    /// Service métier pour la gestion des prêts bancaires.
    /// </summary>
    public class PretService
    {
        private readonly PretRepository _repo = new();
        private readonly PaiementRepository _paiementRepo = new();

        public List<Pret> GetTousPrets() => _repo.GetAll();
        public Pret? GetPret(int id) => _repo.GetById(id);
        public List<Pret> GetPretsByClient(int clientId) => _repo.GetByClientId(clientId);
        public List<Pret> GetPretsByStatut(string statut) => _repo.GetByStatut(statut);

        /// <summary>
        /// Crée un nouveau prêt avec calcul automatique de la mensualité.
        /// </summary>
        public (bool success, string message, int? id) CreerPret(int clientId, decimal montant, decimal taux, int duree, string notes)
        {
            // Vérifier si le client a déjà un prêt en cours
            if (_repo.ClientAPretEnCours(clientId))
                return (false, "Ce client a déjà un prêt actif. Un seul prêt à la fois est autorisé.", null);

            var mensualite = Pret.CalculerMensualite(montant, taux, duree);
            var montantTotal = mensualite * duree;

            var pret = new Pret
            {
                ClientId = clientId,
                Montant = montant,
                TauxInteret = taux,
                DureeMois = duree,
                Mensualite = mensualite,
                MontantTotal = montantTotal,
                Statut = "EnAttente",
                Notes = notes
            };

            int id = _repo.Ajouter(pret);
            return (true, $"Prêt #{id} créé avec succès. Mensualité: {mensualite:N2} USD", id);
        }

        public bool ApprouverPret(int id) => _repo.ModifierStatut(id, "Approuve");
        public bool DemarrerPret(int id) => _repo.ModifierStatut(id, "EnCours");
        public bool RejeterPret(int id) => _repo.ModifierStatut(id, "Rejete");

        /// <summary>
        /// Retourne le montant restant à payer pour un prêt.
        /// </summary>
        public decimal GetMontantRestant(int pretId)
        {
            var pret = _repo.GetById(pretId);
            if (pret == null) return 0;
            var totalPaye = _paiementRepo.GetTotalPaye(pretId);
            return pret.MontantTotal - totalPaye;
        }
    }
}

