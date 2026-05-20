using PretBancaire.Data;
using PretBancaire.Models;
using System.Collections.Generic;

namespace PretBancaire.Services
{
    public class PretService
    {
        private readonly PretRepository _repo = new();
        private readonly PaiementRepository _paiementRepo = new();

        public List<Pret> GetTousPrets() => _repo.GetAll();
        public Pret? GetPret(int id) => _repo.GetById(id);
        public List<Pret> GetPretsByClient(int clientId) => _repo.GetByClientId(clientId);
        public List<Pret> GetPretsByStatut(string statut) => _repo.GetByStatut(statut);

        public (bool success, string message, int? id) CreerPret(int clientId, decimal montant, decimal taux, int duree, string description)
        {
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
                Description = description
            };

            int id = _repo.Ajouter(pret);
            return (true, "Prêt # créé avec succès. Mensualité:  Ar", id);
        }

        public (bool success, string message) ChangerStatutPret(int id, string statut) {
            bool result = _repo.ModifierStatut(id, statut);
            return result ? (true, "Statut mis à jour avec succès.") : (false, "Impossible de mettre à jour le statut.");
        }

        public (bool success, string message) ModifierPret(int id, decimal montant, decimal taux, int duree, string description, string statut)
        {
            var pret = _repo.GetById(id);
            if (pret == null) return (false, "Prêt non trouvé.");

            var mensualite = Pret.CalculerMensualite(montant, taux, duree);
            var montantTotal = mensualite * duree;

            pret.Montant = montant;
            pret.TauxInteret = taux;
            pret.DureeMois = duree;
            pret.Mensualite = mensualite;
            pret.MontantTotal = montantTotal;
            pret.Description = description;
            pret.Statut = statut;

            bool result = _repo.Modifier(pret);
            return result ? (true, "Prêt modifié avec succès.") : (false, "Impossible de modifier le prêt.");
        }

        public (bool success, string message) SupprimerPret(int id)
        {
            var pret = _repo.GetById(id);
            if (pret == null) return (false, "Prêt introuvable.");

            var paiements = _paiementRepo.GetByPretId(id);
            if (paiements.Count > 0)
                return (false, "Impossible de supprimer ce prêt car il possède des paiements associés.");

            bool result = _repo.Supprimer(id);
            return result ? (true, "Prêt supprimé avec succès.") : (false, "Erreur lors de la suppression du prêt.");
        }

        public decimal GetMontantRestant(int pretId)
        {
            var pret = _repo.GetById(pretId);
            if (pret == null) return 0;
            var totalPaye = _paiementRepo.GetTotalPaye(pretId);
            return pret.MontantTotal - totalPaye;
        }
    }
}
