namespace PretBancaire.Models
{
    /// <summary>
    /// Représente un client de la banque.
    /// Correspond à la table 'clients' dans la base de données.
    /// </summary>
    public class Client
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public DateTime DateNaissance { get; set; }
        public string Cin { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateInscription { get; set; } = DateTime.Now;
        public bool Actif { get; set; } = true;

        /// <summary>
        /// Retourne le nom complet du client.
        /// </summary>
        public string NomComplet => $"{Prenom} {Nom}";

        /// <summary>
        /// Calcule l'âge du client.
        /// </summary>
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DateNaissance.Year;
                if (DateNaissance.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        public override string ToString() => $"{NomComplet} (CIN: {Cin})";
    }
}

