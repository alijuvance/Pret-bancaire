namespace PretBancaire.Models
{
    /// <summary>
    /// Représente un paiement (remboursement) effectué sur un prêt.
    /// Correspond à la table 'paiements' dans la base de données.
    /// </summary>
    public class Paiement
    {
        public int Id { get; set; }
        public int PretId { get; set; }
        public decimal Montant { get; set; }
        public DateTime DatePaiement { get; set; } = DateTime.Today;
        public string ModePaiement { get; set; } = "Especes";
        public string Reference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Retourne le libellé du mode de paiement en français.
        /// </summary>
        public string ModePaiementLibelle => ModePaiement switch
        {
            "Especes" => "Espèces",
            "Virement" => "Virement",
            "Cheque" => "Chèque",
            "CarteBancaire" => "Carte Bancaire",
            _ => ModePaiement
        };

        public override string ToString() =>
            $"Paiement #{Id} - {Montant:N2} USD ({ModePaiementLibelle})";
    }
}
