//using MySql.Data.MySqlClient;
using MySqlConnector;
namespace login_store
{
    internal class DatabaseHelper
    {
        private static string connectionString = "server=localhost;user=root;password=;database=football_store;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
