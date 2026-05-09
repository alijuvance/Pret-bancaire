using MySql.Data.MySqlClient;
using PretBancaire.Models;

namespace PretBancaire.Data
{
    /// <summary>
    /// Repository pour les opérations CRUD sur la table 'utilisateurs'.
    /// </summary>
    public class UtilisateurRepository
    {
        /// <summary>
        /// Authentifie un utilisateur par login et mot de passe haché.
        /// </summary>
        public Utilisateur? Authentifier(string login, string motDePasseHash)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                "SELECT * FROM utilisateurs WHERE login = @login AND mot_de_passe = @mdp AND actif = TRUE",
                conn);
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@mdp", motDePasseHash);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapToUtilisateur(reader);

            return null;
        }

        /// <summary>
        /// Récupère tous les utilisateurs.
        /// </summary>
        public List<Utilisateur> GetAll()
        {
            var liste = new List<Utilisateur>();
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand("SELECT * FROM utilisateurs ORDER BY nom, prenom", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                liste.Add(MapToUtilisateur(reader));

            return liste;
        }

        /// <summary>
        /// Récupère un utilisateur par son ID.
        /// </summary>
        public Utilisateur? GetById(int id)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand("SELECT * FROM utilisateurs WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapToUtilisateur(reader);

            return null;
        }

        /// <summary>
        /// Ajoute un nouvel utilisateur dans la base de données.
        /// </summary>
        public int Ajouter(Utilisateur u)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                @"INSERT INTO utilisateurs (nom, prenom, login, mot_de_passe, role, actif)
                  VALUES (@nom, @prenom, @login, @mdp, @role, @actif);
                  SELECT LAST_INSERT_ID();", conn);

            cmd.Parameters.AddWithValue("@nom", u.Nom);
            cmd.Parameters.AddWithValue("@prenom", u.Prenom);
            cmd.Parameters.AddWithValue("@login", u.Login);
            cmd.Parameters.AddWithValue("@mdp", u.MotDePasse);
            cmd.Parameters.AddWithValue("@role", u.Role);
            cmd.Parameters.AddWithValue("@actif", u.Actif);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Met à jour un utilisateur existant.
        /// </summary>
        public bool Modifier(Utilisateur u)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                @"UPDATE utilisateurs SET nom = @nom, prenom = @prenom, login = @login,
                  role = @role, actif = @actif WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("@id", u.Id);
            cmd.Parameters.AddWithValue("@nom", u.Nom);
            cmd.Parameters.AddWithValue("@prenom", u.Prenom);
            cmd.Parameters.AddWithValue("@login", u.Login);
            cmd.Parameters.AddWithValue("@role", u.Role);
            cmd.Parameters.AddWithValue("@actif", u.Actif);

            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Met à jour le mot de passe d'un utilisateur.
        /// </summary>
        public bool ModifierMotDePasse(int id, string motDePasseHash)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                "UPDATE utilisateurs SET mot_de_passe = @mdp WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@mdp", motDePasseHash);

            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Supprime (désactive) un utilisateur.
        /// </summary>
        public bool Supprimer(int id)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                "UPDATE utilisateurs SET actif = FALSE WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Vérifie si un login existe déjà.
        /// </summary>
        public bool LoginExiste(string login, int? excludeId = null)
        {
            using var conn = DatabaseConnection.GetConnection();
            var sql = "SELECT COUNT(*) FROM utilisateurs WHERE login = @login";
            if (excludeId.HasValue) sql += " AND id != @id";

            var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@login", login);
            if (excludeId.HasValue) cmd.Parameters.AddWithValue("@id", excludeId.Value);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        /// <summary>
        /// Mappe un DataReader vers un objet Utilisateur.
        /// </summary>
        private static Utilisateur MapToUtilisateur(MySqlDataReader reader)
        {
            return new Utilisateur
            {
                Id = reader.GetInt32("id"),
                Nom = reader.GetString("nom"),
                Prenom = reader.GetString("prenom"),
                Login = reader.GetString("login"),
                MotDePasse = reader.GetString("mot_de_passe"),
                Role = reader.GetString("role"),
                DateCreation = reader.GetDateTime("date_creation"),
                Actif = reader.GetBoolean("actif")
            };
        }
    }
}

