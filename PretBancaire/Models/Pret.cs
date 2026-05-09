namespace PretBancaire.Models
{
    /// <summary>
    /// Représente un prêt bancaire accordé à un client.
    /// Correspond à la table 'prets' dans la base de données.
    /// </summary>
    public class Pret
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public decimal Montant { get; set; }
        public decimal TauxInteret { get; set; }
        public int DureeMois { get; set; }
        public decimal Mensualite { get; set; }
        public decimal MontantTotal { get; set; }
        public string Statut { get; set; } = "EnAttente";
        public DateTime DateDemande { get; set; } = DateTime.Now;
        public DateTime? DateApprobation { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Propriété de navigation (non stockée en BDD, remplie par jointure)
        public string? NomClient { get; set; }

        /// <summary>
        /// Calcule la mensualité selon la formule d'amortissement :
        /// M = P × [r(1+r)^n] / [(1+r)^n - 1]
        /// </summary>
        /// <param name="montant">Montant emprunté (P)</param>
        /// <param name="tauxAnnuel">Taux d'intérêt annuel en pourcentage</param>
        /// <param name="dureeMois">Durée en mois (n)</param>
        /// <returns>Mensualité calculée</returns>
        public static decimal CalculerMensualite(decimal montant, decimal tauxAnnuel, int dureeMois)
        {
            if (tauxAnnuel == 0)
                return montant / dureeMois;

            // Taux mensuel = taux annuel / 12 / 100
            double r = (double)(tauxAnnuel / 12m / 100m);
            double p = (double)montant;
            int n = dureeMois;

            // Formule : M = P × [r(1+r)^n] / [(1+r)^n - 1]
            double power = Math.Pow(1 + r, n);
            double mensualite = p * (r * power) / (power - 1);

            return Math.Round((decimal)mensualite, 2);
        }

        /// <summary>
        /// Retourne le libellé du statut en français.
        /// </summary>
        public string StatutLibelle => Statut switch
        {
            "EnAttente" => "En Attente",
            "Approuve" => "Approuvé",
            "EnCours" => "En Cours",
            "Termine" => "Terminé",
            "Rejete" => "Rejeté",
            _ => Statut
        };

        public override string ToString() =>
            $"Prêt #{Id} - {Montant:N2} USD - {StatutLibelle}";
    }
}
