using MySql.Data.MySqlClient;
using PretBancaire.Models;

namespace PretBancaire.Data
{
    /// <summary>
    /// Repository pour les opérations CRUD sur la table 'clients'.
    /// </summary>
    public class ClientRepository
    {
        /// <summary>
        /// Récupère tous les clients actifs.
        /// </summary>
        public List<Client> GetAll()
        {
            var liste = new List<Client>();
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                "SELECT * FROM clients WHERE actif = TRUE ORDER BY nom, prenom", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                liste.Add(MapToClient(reader));

            return liste;
        }

        /// <summary>
        /// Récupère un client par son ID.
        /// </summary>
        public Client? GetById(int id)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand("SELECT * FROM clients WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapToClient(reader);

            return null;
        }

        /// <summary>
        /// Recherche des clients par nom, prénom ou CIN.
        /// </summary>
        public List<Client> Rechercher(string terme)
        {
            var liste = new List<Client>();
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                @"SELECT * FROM clients 
                  WHERE actif = TRUE AND (nom LIKE @terme OR prenom LIKE @terme OR cin LIKE @terme)
                  ORDER BY nom, prenom", conn);
            cmd.Parameters.AddWithValue("@terme", $"%{terme}%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                liste.Add(MapToClient(reader));

            return liste;
        }

        /// <summary>
        /// Ajoute un nouveau client.
        /// </summary>
        public int Ajouter(Client c)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                @"INSERT INTO clients (nom, prenom, cin, telephone, adresse, email)
                  VALUES (@nom, @prenom, @cin, @tel, @adr, @email);
                  SELECT LAST_INSERT_ID();", conn);

            cmd.Parameters.AddWithValue("@nom", c.Nom);
            cmd.Parameters.AddWithValue("@prenom", c.Prenom);
            cmd.Parameters.AddWithValue("@cin", c.Cin);
            cmd.Parameters.AddWithValue("@tel", c.Telephone);
            cmd.Parameters.AddWithValue("@adr", c.Adresse);
            cmd.Parameters.AddWithValue("@email", c.Email);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Met à jour un client existant.
        /// </summary>
        public bool Modifier(Client c)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                @"UPDATE clients SET nom = @nom, prenom = @prenom,
                  cin = @cin, telephone = @tel, adresse = @adr, email = @email
                  WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("@id", c.Id);
            cmd.Parameters.AddWithValue("@nom", c.Nom);
            cmd.Parameters.AddWithValue("@prenom", c.Prenom);
            cmd.Parameters.AddWithValue("@cin", c.Cin);
            cmd.Parameters.AddWithValue("@tel", c.Telephone);
            cmd.Parameters.AddWithValue("@adr", c.Adresse);
            cmd.Parameters.AddWithValue("@email", c.Email);

            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Supprime (désactive) un client.
        /// Vérifie d'abord qu'il n'a pas de prêt actif.
        /// </summary>
        public bool Supprimer(int id)
        {
            using var conn = DatabaseConnection.GetConnection();

            // Vérifier les prêts actifs
            var checkCmd = new MySqlCommand(
                "SELECT COUNT(*) FROM prets WHERE client_id = @id AND statut IN ('EnAttente','Approuve','EnCours')",
                conn);
            checkCmd.Parameters.AddWithValue("@id", id);
            int pretsActifs = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (pretsActifs > 0)
                return false;  // Impossible de supprimer : prêts actifs existants

            var cmd = new MySqlCommand(
                "UPDATE clients SET actif = FALSE WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Vérifie si un CIN existe déjà.
        /// </summary>
        public bool CinExiste(string cin, int? excludeId = null)
        {
            using var conn = DatabaseConnection.GetConnection();
            var sql = "SELECT COUNT(*) FROM clients WHERE cin = @cin";
            if (excludeId.HasValue) sql += " AND id != @id";

            var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@cin", cin);
            if (excludeId.HasValue) cmd.Parameters.AddWithValue("@id", excludeId.Value);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        /// <summary>
        /// Compte le nombre total de clients actifs.
        /// </summary>
        public int Count()
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand("SELECT COUNT(*) FROM clients WHERE actif = TRUE", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Mappe un DataReader vers un objet Client.
        /// </summary>
        private static Client MapToClient(MySqlDataReader reader)
        {
            return new Client
            {
                Id = reader.GetInt32("id"),
                Nom = reader.GetString("nom"),
                Prenom = reader.GetString("prenom"),
                Cin = reader.GetString("cin"),
                Telephone = reader.GetString("telephone"),
                Adresse = reader.IsDBNull(reader.GetOrdinal("adresse")) ? "" : reader.GetString("adresse"),
                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString("email"),
                DateInscription = reader.GetDateTime("date_inscription"),
                Actif = reader.GetBoolean("actif")
            };
        }
    }
}

