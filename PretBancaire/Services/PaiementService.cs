using PretBancaire.Data;
using PretBancaire.Models;
using System.Collections.Generic;

namespace PretBancaire.Services
{
    public class PaiementService
    {
        private readonly PaiementRepository _repo = new();
        private readonly PretRepository _pretRepo = new();

        public List<Paiement> GetTousPaiements() => _repo.GetAll();
        public List<Paiement> GetPaiementsByPret(int pretId) => _repo.GetByPretId(pretId);
        public decimal GetTotalPaiements(int pretId) => _repo.GetTotalPaye(pretId);

        public (bool success, string message) EnregistrerPaiement(Paiement paiement)
        {
            var pret = _pretRepo.GetById(paiement.PretId);
            if (pret == null)
                return (false, "Prêt introuvable.");

            if (pret.Statut != "EnCours")
                return (false, "Le paiement n'est possible que sur un prêt 'En Cours'.");

            decimal totalPaye = _repo.GetTotalPaye(paiement.PretId);
            decimal restant = pret.MontantTotal - totalPaye;

            if (paiement.Montant > restant)
                return (false, $"Le montant dépasse le restant dû ({restant:N2} USD).");

            if (paiement.Montant <= 0)
                return (false, "Le montant doit être supérieur à 0.");

            int id = _repo.Ajouter(paiement);

            decimal nouveauTotal = totalPaye + paiement.Montant;
            if (nouveauTotal >= pret.MontantTotal)
                _pretRepo.ModifierStatut(pret.Id, "Termine");

            return (true, $"Paiement #{id} enregistré. Restant: {(restant - paiement.Montant):N2} USD");
        }
    }
}
