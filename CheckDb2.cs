using System;
using MySql.Data.MySqlClient;

class Program
{
    static void Main()
    {
        string connStr = "Server=localhost;Port=3306;Database=pret_bancaire;Uid=root;Pwd=ali9114;";
        using (var conn = new MySqlConnection(connStr))
        {
            conn.Open();
            var cmd = new MySqlCommand("SELECT id, login, mot_de_passe, role, actif FROM utilisateurs", conn);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["id"]}, Login: {reader["login"]}, Pass: {reader["mot_de_passe"]}, Actif: {reader["actif"]}");
                }
            }
        }
    }
}
