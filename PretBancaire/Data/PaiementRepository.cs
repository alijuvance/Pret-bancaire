using MySql.Data.MySqlClient;
using PretBancaire.Models;
using System;
using System.Collections.Generic;

namespace PretBancaire.Data
{
    public class PaiementRepository
    {
        public List<Paiement> GetAll()
        {
            var liste = new List<Paiement>();
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                @"SELECT p.*, c.nom as nom_client, c.prenom as prenom_client 
                  FROM paiements p 
                  JOIN prets pr ON p.pret_id = pr.id 
                  JOIN clients c ON pr.client_id = c.id 
                  ORDER BY p.date_paiement DESC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var p = MapToPaiement(reader);
                p.NomClient = $"{reader["nom_client"]} {reader["prenom_client"]}";
                liste.Add(p);
            }
            return liste;
        }

        public List<Paiement> GetByPretId(int pretId)
        {
            var liste = new List<Paiement>();
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand("SELECT * FROM paiements WHERE pret_id = @pid ORDER BY date_paiement DESC", conn);
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
        public bool Modifier(Paiement p)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand(
                @"UPDATE paiements SET montant = @m, date_paiement = @dp, mode_paiement = @mp, 
                  reference = @ref, notes = @n WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", p.Id);
            cmd.Parameters.AddWithValue("@m", p.Montant);
            cmd.Parameters.AddWithValue("@dp", p.DatePaiement);
            cmd.Parameters.AddWithValue("@mp", p.ModePaiement);
            cmd.Parameters.AddWithValue("@ref", p.Reference);
            cmd.Parameters.AddWithValue("@n", p.Notes);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Supprimer(int id)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand("DELETE FROM paiements WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public decimal GetTotalPaye(int pretId)
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand("SELECT COALESCE(SUM(montant), 0) FROM paiements WHERE pret_id = @pid", conn);
            cmd.Parameters.AddWithValue("@pid", pretId);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public decimal GetTotalRemboursements()
        {
            using var conn = DatabaseConnection.GetConnection();
            var cmd = new MySqlCommand("SELECT COALESCE(SUM(montant), 0) FROM paiements", conn);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

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
