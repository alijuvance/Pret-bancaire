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
            var cmd = new MySqlCommand("DELETE FROM paiements; DELETE FROM prets; DELETE FROM clients; ALTER TABLE paiements AUTO_INCREMENT = 1; ALTER TABLE prets AUTO_INCREMENT = 1; ALTER TABLE clients AUTO_INCREMENT = 1;", conn);
            int rows = cmd.ExecuteNonQuery();
            Console.WriteLine($"Mock data deleted. Rows affected: {rows}");
        }
    }
}
