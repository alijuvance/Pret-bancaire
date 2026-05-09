namespace PretBancaire.Models
{
    /// <summary>
    /// Représente un utilisateur du système (Admin ou Agent bancaire).
    /// Correspond à la table 'utilisateurs' dans la base de données.
    /// </summary>
    public class Utilisateur
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string MotDePasse { get; set; } = string.Empty;
        public string Role { get; set; } = "Agent";  // "Admin" ou "Agent"
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public bool Actif { get; set; } = true;

        /// <summary>
        /// Retourne le nom complet de l'utilisateur.
        /// </summary>
        public string NomComplet => $"{Prenom} {Nom}";

        /// <summary>
        /// Vérifie si l'utilisateur est administrateur.
        /// </summary>
        public bool EstAdmin => Role == "Admin";

        public override string ToString() => $"{NomComplet} ({Role})";
    }
}

