using System.Security.Cryptography;
using System.Text;

namespace PretBancaire.Utils
{
    /// <summary>
    /// Utilitaire pour le hachage sécurisé des mots de passe.
    /// Utilise SHA-256 avec un sel (salt) pour renforcer la sécurité.
    /// </summary>
    public static class PasswordHelper
    {
        // Sel utilisé pour le hachage (en production, utiliser un sel unique par utilisateur)
        private const string Salt = "PretBancaire_";

        /// <summary>
        /// Hache un mot de passe en utilisant SHA-256 avec sel.
        /// </summary>
        /// <param name="motDePasse">Le mot de passe en clair</param>
        /// <returns>Le hash hexadécimal du mot de passe</returns>
        public static string Hacher(string motDePasse)
        {
            string salted = Salt + motDePasse;
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(salted));
            var sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// Vérifie si un mot de passe en clair correspond à un hash.
        /// </summary>
        public static bool Verifier(string motDePasse, string hash)
        {
            return Hacher(motDePasse) == hash;
        }
    }
}
