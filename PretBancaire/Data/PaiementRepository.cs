using MySql.Data.MySqlClient;
using PretBancaire.Models;

namespace PretBancaire.Data
{
    /// <summary>
    /// Repository pour les opérations CRUD sur la table 'paiements'.
    /// </summary>
    public class PaiementRepository
    {
        public List<Paiement> GetByPretId(int pretId)
        {
            var liste = new List<Paiement>();
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                "SELECT * FROM paiements WHERE pret_id = @pid ORDER BY date_paiement DESC", conn);
            cmd.Parameters.AddWithValue("@pid", pretId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) liste.Add(MapToPaiement(reader));
            return liste;
        }

        public int Ajouter(Paiement p)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                @"INSERT INTO paiements (pret_id, montant, date_paiement, mode_paiement, reference, notes)
                  VALUES (@pid, @m, @dp, @mp, @ref, @n); SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@pid", p.PretId);
            cmd.Parameters.AddWithValue("@m", p.Montant);
            cmd.Parameters.AddWithValue("@dp", p.DatePaiement);
            cmd.Parameters.AddWithValue("@mp", p.ModePaiement);
            cmd.Parameters.AddWithValue("@ref", p.Reference);
            cmd.Parameters.AddWithValue("@n", p.Notes);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Calcule le total des paiements pour un prêt donné.
        /// </summary>
        public decimal GetTotalPaye(int pretId)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                "SELECT COALESCE(SUM(montant), 0) FROM paiements WHERE pret_id = @pid", conn);
            cmd.Parameters.AddWithValue("@pid", pretId);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Calcule le montant total de tous les remboursements.
        /// </summary>
        public decimal GetTotalRemboursements()
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand("SELECT COALESCE(SUM(montant), 0) FROM paiements", conn);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Compte le nombre total de paiements.
        /// </summary>
        public int Count()
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand("SELECT COUNT(*) FROM paiements", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static Paiement MapToPaiement(MySqlDataReader reader)
        {
            return new Paiement
            {
                Id = reader.GetInt32("id"),
                PretId = reader.GetInt32("pret_id"),
                Montant = reader.GetDecimal("montant"),
                DatePaiement = reader.GetDateTime("date_paiement"),
                ModePaiement = reader.GetString("mode_paiement"),
                Reference = reader.IsDBNull(reader.GetOrdinal("reference")) ? "" : reader.GetString("reference"),
                Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? "" : reader.GetString("notes")
            };
        }
    }
}

