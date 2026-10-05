using Home_Food_and_Beverage.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home_Food_and_Beverage.Data
{
    internal class OrderRepository
    {
        public int AddOrder(Orders order)
        {
            string query = @"INSERT INTO orders (staff_id, order_date, status, total_amount, payment_method)
                              VALUES (@StaffId, @OrderDate, @Status, @TotalAmount, @PaymentMethod);
                              SELECT LAST_INSERT_ID();";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@StaffId", order.StaffId);
                cmd.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@Status", "Pending");
                cmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                cmd.Parameters.AddWithValue("@PaymentMethod", order.PaymentMethod ?? "");

                conn.Open();
                return (int)(ulong)cmd.ExecuteScalar();
            }
        }

        public void AddOrderItem(OrderItem item)
        {
            string query = @"INSERT INTO order_items (order_id, recipe_id, item_name, quantity, price, special_request)
                              VALUES (@OrderId, @RecipeId, @ItemName, @Quantity, @Price, @SpecialRequest)";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@OrderId", item.OrderId);
                cmd.Parameters.AddWithValue("@RecipeId", item.RecipeId);
                cmd.Parameters.AddWithValue("@ItemName", item.ItemName);
                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                cmd.Parameters.AddWithValue("@Price", item.Price);
                cmd.Parameters.AddWithValue("@SpecialRequest", item.SpecialRequest ?? "");

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<Orders> GetAllOrders()
        {
            List<Orders> orders = new List<Orders>();
            string query = "SELECT id, staff_id, order_date, status, total_amount, payment_method FROM orders ORDER BY order_date DESC";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    orders.Add(MapOrder(reader));
            }
            return orders;
        }

        public List<Orders> GetOrdersByDateRange(DateTime start, DateTime end)
        {
            List<Orders> orders = new List<Orders>();
            string query = @"SELECT id, staff_id, order_date, status, total_amount, payment_method 
                              FROM orders WHERE order_date BETWEEN @Start AND @End ORDER BY order_date DESC";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@End", end);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    orders.Add(MapOrder(reader));
            }
            return orders;
        }

        public void UpdateOrderStatus(int orderId, string newStatus)
        {
            string query = "UPDATE orders SET status = @Status WHERE id = @Id";
            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", orderId);
                cmd.Parameters.AddWithValue("@Status", newStatus);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private Orders MapOrder(MySqlDataReader reader)
        {
            return new Orders
            {
                Id = reader.GetInt32("id"),
                StaffId = reader.GetInt32("staff_id"),
                OrderDate = reader.GetDateTime("order_date"),
                Status = reader["status"].ToString(),
                TotalAmount = reader.GetDecimal("total_amount"),
                PaymentMethod = reader["payment_method"].ToString()
            };
        }

        public List<Orders> SearchOrders(string dateRange, string keyword)
        {
            DateTime start, end;
            DateTime today = DateTime.Today;

            switch (dateRange)
            {
                case "This Week":
                    start = today.AddDays(-(int)today.DayOfWeek);
                    end = start.AddDays(7).AddSeconds(-1);
                    break;
                case "This Month":
                    start = new DateTime(today.Year, today.Month, 1);
                    end = start.AddMonths(1).AddSeconds(-1);
                    break;
                default: // "Today"
                    start = today;
                    end = today.AddDays(1).AddSeconds(-1);
                    break;
            }

            string whereClause = "WHERE order_date BETWEEN @Start AND @End";
            if (!string.IsNullOrWhiteSpace(keyword))
                whereClause += " AND (payment_method LIKE @Keyword OR staff_id LIKE @Keyword)";

            List<Orders> orders = new List<Orders>();
            string query = $@"SELECT id, staff_id, order_date, status, total_amount, payment_method 
                       FROM orders {whereClause}
                       ORDER BY order_date DESC";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@End", end);
                if (!string.IsNullOrWhiteSpace(keyword))
                    cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    orders.Add(MapOrder(reader));
            }
            return orders;
        }

        public string GetItemSummary(int orderId)
        {
            string query = "SELECT item_name, quantity FROM order_items WHERE order_id = @OrderId";
            List<string> parts = new List<string>();

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    parts.Add($"{reader.GetString("item_name")}x{reader.GetInt32("quantity")}");
            }
            return string.Join(", ", parts);
        }

    }
}
