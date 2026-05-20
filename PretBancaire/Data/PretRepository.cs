using MySql.Data.MySqlClient;
using PretBancaire.Models;

namespace PretBancaire.Data
{
    /// <summary>
    /// Repository pour les opérations CRUD sur la table 'prets'.
    /// </summary>
    public class PretRepository
    {
        public List<Pret> GetAll()
        {
            var liste = new List<Pret>();
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                @"SELECT p.*, CONCAT(c.prenom, ' ', c.nom) AS nom_client
                  FROM prets p INNER JOIN clients c ON p.client_id = c.id
                  ORDER BY p.date_demande DESC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) liste.Add(MapToPret(reader, true));
            return liste;
        }

        public Pret? GetById(int id)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                @"SELECT p.*, CONCAT(c.prenom, ' ', c.nom) AS nom_client
                  FROM prets p INNER JOIN clients c ON p.client_id = c.id
                  WHERE p.id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapToPret(reader, true) : null;
        }

        public List<Pret> GetByClientId(int clientId)
        {
            var liste = new List<Pret>();
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                "SELECT * FROM prets WHERE client_id = @cid ORDER BY date_demande DESC", conn);
            cmd.Parameters.AddWithValue("@cid", clientId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) liste.Add(MapToPret(reader));
            return liste;
        }

        public List<Pret> GetByStatut(string statut)
        {
            var liste = new List<Pret>();
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                @"SELECT p.*, CONCAT(c.prenom, ' ', c.nom) AS nom_client
                  FROM prets p INNER JOIN clients c ON p.client_id = c.id
                  WHERE p.statut = @s ORDER BY p.date_demande DESC", conn);
            cmd.Parameters.AddWithValue("@s", statut);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) liste.Add(MapToPret(reader, true));
            return liste;
        }

        public int Ajouter(Pret p)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                @"INSERT INTO prets (client_id, montant, taux_interet, duree_mois, mensualite, montant_total, statut, description, date_approbation)
                  VALUES (@cid, @m, @t, @d, @men, @tot, 'EnCours', @desc, NOW()); SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@cid", p.ClientId);
            cmd.Parameters.AddWithValue("@m", p.Montant);
            cmd.Parameters.AddWithValue("@t", p.TauxInteret);
            cmd.Parameters.AddWithValue("@d", p.DureeMois);
            cmd.Parameters.AddWithValue("@men", p.Mensualite);
            cmd.Parameters.AddWithValue("@tot", p.MontantTotal);
            cmd.Parameters.AddWithValue("@desc", p.Description);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool ModifierStatut(int id, string nouveauStatut)
        {
            using var conn = DatabaseConnection.GetConnection();
            var sql = "UPDATE prets SET statut = @s";
            if (nouveauStatut == "Approuve" || nouveauStatut == "EnCours")
                sql += ", date_approbation = NOW()";
            sql += " WHERE id = @id";
            var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@s", nouveauStatut);
            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Modifie les détails d'un prêt.
        /// </summary>
        public bool Modifier(Pret p)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                @"UPDATE prets 
                  SET montant = @montant, montant_total = @montant_total, taux_interet = @taux, duree_mois = @duree, 
                      mensualite = @mensualite, description = @desc, statut = @statut
                  WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("@id", p.Id);
            cmd.Parameters.AddWithValue("@montant", p.Montant);
            cmd.Parameters.AddWithValue("@montant_total", p.MontantTotal);
            cmd.Parameters.AddWithValue("@taux", p.TauxInteret);
            cmd.Parameters.AddWithValue("@duree", p.DureeMois);
            cmd.Parameters.AddWithValue("@mensualite", p.Mensualite);
            cmd.Parameters.AddWithValue("@desc", string.IsNullOrWhiteSpace(p.Description) ? DBNull.Value : p.Description);
            cmd.Parameters.AddWithValue("@statut", p.Statut);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Supprimer(int id)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand("DELETE FROM prets WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool ClientAPretEnCours(int clientId)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM prets WHERE client_id = @id AND statut IN ('EnAttente','Approuve','EnCours')", conn);
            cmd.Parameters.AddWithValue("@id", clientId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public Dictionary<string, int> CountByStatut()
        {
            var result = new Dictionary<string, int>();
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand("SELECT statut, COUNT(*) as total FROM prets GROUP BY statut", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) result[reader.GetString("statut")] = reader.GetInt32("total");
            return result;
        }

        public decimal GetMontantTotalEnCours()
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                "SELECT COALESCE(SUM(montant_total), 0) FROM prets WHERE statut IN ('Approuve','EnCours')", conn);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        private static Pret MapToPret(MySqlDataReader reader, bool withClient = false)
        {
            var pret = new Pret
            {
                Id = reader.GetInt32("id"),
                ClientId = reader.GetInt32("client_id"),
                Montant = reader.GetDecimal("montant"),
                TauxInteret = reader.GetDecimal("taux_interet"),
                DureeMois = reader.GetInt32("duree_mois"),
                Mensualite = reader.GetDecimal("mensualite"),
                MontantTotal = reader.GetDecimal("montant_total"),
                Statut = reader.GetString("statut"),
                DateDemande = reader.GetDateTime("date_demande"),
                DateApprobation = reader.IsDBNull(reader.GetOrdinal("date_approbation"))
                    ? null : reader.GetDateTime("date_approbation"),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description")
            };
            if (withClient) pret.NomClient = reader.GetString("nom_client");
            return pret;
        }
    }
}

