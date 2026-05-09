using System.Text.RegularExpressions;

namespace PretBancaire.Utils
{
    /// <summary>
    /// Utilitaire de validation des données saisies par l'utilisateur.
    /// </summary>
    public static class ValidationHelper
    {
        public static bool EstNonVide(string? valeur) =>
            !string.IsNullOrWhiteSpace(valeur);

        public static bool EstEmailValide(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return true; // Email optionnel
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public static bool EstTelephoneValide(string? tel)
        {
            if (string.IsNullOrWhiteSpace(tel)) return false;
            return Regex.IsMatch(tel, @"^[\+]?[\d\s\-]{8,20}$");
        }

        public static bool EstMontantValide(string? montant)
        {
            return decimal.TryParse(montant, out decimal val) && val > 0;
        }

        public static bool EstTauxValide(string? taux)
        {
            return decimal.TryParse(taux, out decimal val) && val >= 0 && val <= 100;
        }

        public static bool EstDureeValide(string? duree)
        {
            return int.TryParse(duree, out int val) && val > 0 && val <= 360;
        }

        /// <summary>
        /// Valide un formulaire client et retourne les erreurs.
        /// </summary>
        public static List<string> ValiderClient(string nom, string prenom, string cin, string tel, string? email)
        {
            var erreurs = new List<string>();
            if (!EstNonVide(nom)) erreurs.Add("Le nom est obligatoire.");
            if (!EstNonVide(prenom)) erreurs.Add("Le prénom est obligatoire.");
            if (!EstNonVide(cin)) erreurs.Add("Le CIN est obligatoire.");
            if (!EstTelephoneValide(tel)) erreurs.Add("Le téléphone est invalide.");
            if (!EstEmailValide(email)) erreurs.Add("L'email est invalide.");
            return erreurs;
        }

        /// <summary>
        /// Valide un formulaire de prêt et retourne les erreurs.
        /// </summary>
        public static List<string> ValiderPret(string montant, string taux, string duree, int? clientId)
        {
            var erreurs = new List<string>();
            if (!clientId.HasValue || clientId <= 0) erreurs.Add("Veuillez sélectionner un client.");
            if (!EstMontantValide(montant)) erreurs.Add("Le montant est invalide (doit être > 0).");
            if (!EstTauxValide(taux)) erreurs.Add("Le taux d'intérêt est invalide (0-100%).");
            if (!EstDureeValide(duree)) erreurs.Add("La durée est invalide (1-360 mois).");
            return erreurs;
        }
    }
}
