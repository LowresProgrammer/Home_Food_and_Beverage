using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Home_Food_and_Beverage.Models;

namespace Home_Food_and_Beverage.Data
{
    internal class SalesRepository
    {
        public int GetOrderCount(DateTime date)
        {
            string query = "SELECT COUNT(*) FROM orders WHERE DATE(order_date) = DATE(@Date)";
            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Date", date);
                conn.Open();
                return (int)(long)cmd.ExecuteScalar();
            }
        }

        public decimal GetTotalByPaymentMethod(DateTime date, string method)
        {
            string query = @"SELECT IFNULL (SUM(total_amount), 0) FROM orders WHERE DATE(order_date) = DATE(@Date) AND payment_method = @Method";
            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Date", date);
                cmd.Parameters.AddWithValue("@Method", method);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
            }
        }

        public async Task<decimal> GetRevenueAsync(DateTime date)
        {
            // Run the DB query on a background thread to avoid blocking the UI thread
            return await Task.Run(() =>
            {
                string query = @"SELECT IFNULL(SUM(total_amount), 0) FROM orders WHERE DATE(order_date) = DATE(@Date)";
                using (MySqlConnection conn = DatabaseConnection.GetConnection())
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Date", date);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            });
        }
    }
}
