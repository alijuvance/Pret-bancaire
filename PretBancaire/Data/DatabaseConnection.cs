using MySql.Data.MySqlClient;

namespace PretBancaire.Data
{
    /// <summary>
    /// Gère la connexion à la base de données MySQL.
    /// Utilise le pattern Singleton pour centraliser la configuration.
    /// </summary>
    public static class DatabaseConnection
    {
        // ====================================================================
        // CONFIGURATION DE CONNEXION — Modifiez ces valeurs selon votre
        // environnement MySQL local.
        // ====================================================================
        private const string Server = "localhost";
        private const string Database = "pret_bancaire";
        private const string User = "root";
        private const string Password = "ali9114";  // Mot de passe MySQL
        private const int Port = 3306;

        /// <summary>
        /// Chaîne de connexion MySQL construite à partir des constantes.
        /// </summary>
        private static string ConnectionString =>
            $"Server={Server};Port={Port};Database={Database};Uid={User};Pwd={Password};" +
            $"CharSet=utf8mb4;SslMode=Preferred;ConnectionTimeout=30;" +
            $"AllowPublicKeyRetrieval=True;Pooling=True;MinimumPoolSize=1;MaximumPoolSize=20;";

        /// <summary>
        /// Crée et retourne une nouvelle connexion MySQL.
        /// Chaque appel crée une nouvelle connexion qui doit être fermée après usage.
        /// Utilisation recommandée : using(var conn = DatabaseConnection.GetConnection()) { ... }
        /// </summary>
        /// <returns>Une nouvelle connexion MySQL ouverte</returns>
        public static MySqlConnection GetConnection()
        {
            var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Teste la connexion à la base de données.
        /// </summary>
        /// <returns>True si la connexion réussit, False sinon</returns>
        public static bool TestConnection()
        {
            try
            {
                using var connection = GetConnection();
                return connection.State == System.Data.ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Retourne un message décrivant l'état de la connexion.
        /// </summary>
        public static string GetConnectionStatus()
        {
            try
            {
                using var connection = GetConnection();
                return $"✅ Connecté à MySQL {connection.ServerVersion} — Base: {Database}";
            }
            catch (Exception ex)
            {
                return $"❌ Erreur de connexion: {ex.Message}";
            }
        }
    }
}

