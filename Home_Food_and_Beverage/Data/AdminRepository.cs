using Home_Food_and_Beverage.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home_Food_and_Beverage.Data
{
    internal class AdminRepository
    {
        public AdminUser ValidateCredentials(string username, string password, string twoFactorCode)
        {
            AdminUser admin = null;
            string query = @"SELECT id, username, password, two_factor_code, name 
                              FROM admin_users WHERE username = @Username";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string storedPassword = reader.GetString("password");
                    string storedCode = reader.GetString("two_factor_code");

                    // Plain text comparison — simple, no hashing
                    if (storedPassword == password && storedCode == twoFactorCode)
                    {
                        admin = new AdminUser
                        {
                            Id = reader.GetInt32("id"),
                            Username = reader.GetString("username"),
                            PasswordHash = storedPassword,
                            TwoFactorCode = storedCode,
                            Name = reader.GetString("name")
                        };
                    }
                }
            }
            return admin;
        }


    }
}
