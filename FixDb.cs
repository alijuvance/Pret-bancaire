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
            var cmd = new MySqlCommand("UPDATE utilisateurs SET actif = 1 WHERE login = 'admin'", conn);
            int rows = cmd.ExecuteNonQuery();
            Console.WriteLine($"Rows affected: {rows}");
        }
    }
}
