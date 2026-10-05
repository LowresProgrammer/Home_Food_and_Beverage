using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Home_Food_and_Beverage.Models;
using Home_Food_and_Beverage.Data;

namespace Home_Food_and_Beverage.Data
{
    internal class StaffRepository
    {
        public Models.Staff ValidateLogin(string staffId, string pin)
        {
            Models.Staff staff = null;
            string query = "SELECT id, name, pin, role FROM staff WHERE id = @Id AND pin = @Pin";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", staffId);
                cmd.Parameters.AddWithValue("@Pin", pin);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                    staff = MapStaff(reader);
            }
            return staff;
        }

        public Models.Staff GetStaffById(int id)
        {
            Models.Staff staff = null;
            string query = "SELECT id, name, pin, role FROM staff WHERE id = @Id";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                    staff = MapStaff(reader);
            }
            return staff;
        }

        public List<Models.Staff> SearchStaff(string role, string keyword)
        {
            List<Models.Staff> list = new List<Models.Staff>();
            string query = "SELECT id, name, pin, role FROM staff WHERE 1=1";
            if (role != "All") query += " AND role = @Role";
            if (!string.IsNullOrEmpty(keyword)) query += " AND name LIKE @Keyword";
            query += " ORDER BY name";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                if (role != "All") cmd.Parameters.AddWithValue("@Role", role);
                if (!string.IsNullOrEmpty(keyword)) cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapStaff(reader));
            }
            return list;
        }

        public List<Models.Staff> GetAllStaff()
        {
            List<Models.Staff> list = new List<Models.Staff>();
            string query = "SELECT id, name, pin, role FROM staff ORDER BY id";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapStaff(reader));
            }
            return list;
        }

        public void AddStaff(Models.Staff staff)
        {
            string query = "INSERT INTO staff (name, pin, role) VALUES (@Name, @Pin, @Role)";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Name", staff.Name);
                cmd.Parameters.AddWithValue("@Pin", staff.Pin);
                cmd.Parameters.AddWithValue("@Role", staff.Role);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateStaff(Models.Staff staff)
        {
            string query = "UPDATE staff SET name=@Name, pin=@Pin, role=@Role WHERE id=@Id";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", staff.Id);
                cmd.Parameters.AddWithValue("@Name", staff.Name);
                cmd.Parameters.AddWithValue("@Pin", staff.Pin);
                cmd.Parameters.AddWithValue("@Role", staff.Role);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteStaff(int id)
        {
            string query = "DELETE FROM staff WHERE id = @Id";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private Models.Staff MapStaff(MySqlDataReader reader)
        {
            return new Models.Staff
            {
                Id = reader.GetInt32("id"),
                Name = reader["name"].ToString(),
                Pin = reader["pin"].ToString(),
                Role = reader["role"].ToString()
            };
        }

    }
}
