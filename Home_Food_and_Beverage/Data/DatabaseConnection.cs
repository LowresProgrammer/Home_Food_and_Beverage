using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Home_Food_and_Beverage.Data
{
    internal class DatabaseConnection
    {
        private static string connectionString =
            "server=localhost;Uid=root;Pwd=;database=foodandbeverage;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
