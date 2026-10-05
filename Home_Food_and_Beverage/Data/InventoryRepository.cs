using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Home_Food_and_Beverage.Models;

namespace Home_Food_and_Beverage.Data
{
    internal class InventoryRepository
    {
        private const string AllColumns =
            "id, item_name, quantity, unit, low_stock_threshold, last_updated, category, dietary_status, storage_location, unit_cost";

        public List<InventoryItem> GetAllItems()
        {
            return SearchItems("All", "All", "All", "All", "");
        }

        public List<InventoryItem> SearchItems(string category, string dietaryStatus, string storageLocation, string dateRange, string keyword)
        {
            List<InventoryItem> items = new List<InventoryItem>();
            string query = $"SELECT {AllColumns} FROM inventory_items WHERE 1=1";

            if (category != "All") query += " AND category = @Category";
            if (dietaryStatus != "All") query += " AND dietary_status = @DietaryStatus";
            if (storageLocation != "All") query += " AND storage_location = @StorageLocation";
            if (!string.IsNullOrWhiteSpace(keyword)) query += " AND item_name LIKE @Keyword";

            DateTime? rangeStart = GetRangeStart(dateRange);
            if (rangeStart.HasValue) query += " AND last_updated >= @RangeStart";

            query += " ORDER BY item_name";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                if (category != "All") cmd.Parameters.AddWithValue("@Category", category);
                if (dietaryStatus != "All") cmd.Parameters.AddWithValue("@DietaryStatus", dietaryStatus);
                if (storageLocation != "All") cmd.Parameters.AddWithValue("@StorageLocation", storageLocation);
                if (!string.IsNullOrWhiteSpace(keyword)) cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");
                if (rangeStart.HasValue) cmd.Parameters.AddWithValue("@RangeStart", rangeStart.Value);

                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    items.Add(MapItem(reader));
            }
            return items;
        }

        private DateTime? GetRangeStart(string dateRange)
        {
            DateTime today = DateTime.Today;
            switch (dateRange)
            {
                case "Today": return today;
                case "This Week": return today.AddDays(-(int)today.DayOfWeek);
                case "This Month": return new DateTime(today.Year, today.Month, 1);
                default: return null; // "All"
            }
        }

        public List<InventoryItem> GetLowStockItems()
        {
            List<InventoryItem> items = new List<InventoryItem>();
            string query = $"SELECT {AllColumns} FROM inventory_items WHERE quantity <= low_stock_threshold ORDER BY item_name";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    items.Add(MapItem(reader));
            }
            return items;
        }

        public int GetTotalIngredientCount()
        {
            string query = "SELECT COUNT(*) FROM inventory_items";
            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public decimal GetTotalStockValue()
        {
            string query = "SELECT IFNULL(SUM(quantity * unit_cost), 0) FROM inventory_items";
            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
            }
        }

        public int GetLowStockCount()
        {
            string query = "SELECT COUNT(*) FROM inventory_items WHERE quantity <= low_stock_threshold";
            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // "+ Add New Item"
        public void AddItem(InventoryItem item)
        {
            string query = @"INSERT INTO inventory_items 
                              (item_name, quantity, unit, low_stock_threshold, last_updated, category, dietary_status, storage_location, unit_cost)
                              VALUES (@Name, @Qty, @Unit, @Threshold, @Now, @Category, @Dietary, @Storage, @Cost)";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Name", item.ItemName);
                cmd.Parameters.AddWithValue("@Qty", item.Quantity);
                cmd.Parameters.AddWithValue("@Unit", item.Unit);
                cmd.Parameters.AddWithValue("@Threshold", item.LowStockThreshold);
                cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                cmd.Parameters.AddWithValue("@Category", item.Category ?? "");
                cmd.Parameters.AddWithValue("@Dietary", item.DietaryStatus ?? "");
                cmd.Parameters.AddWithValue("@Storage", item.StorageLocation ?? "");
                cmd.Parameters.AddWithValue("@Cost", item.UnitCost);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // "Receive Stock" / "Reorder" - adds to existing quantity
        public void ReceiveStock(int itemId, int quantityReceived)
        {
            string query = "UPDATE inventory_items SET quantity = quantity + @Qty, last_updated = @Now WHERE id = @Id";
            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Qty", quantityReceived);
                cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                cmd.Parameters.AddWithValue("@Id", itemId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeductStock(string itemName, int quantityUsed)
        {
            string query = "UPDATE inventory_items SET quantity = quantity - @Qty, last_updated = @Now WHERE item_name = @Name";
            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Qty", quantityUsed);
                cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                cmd.Parameters.AddWithValue("@Name", itemName);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteItem(int id)
        {
            string query = "DELETE FROM inventory_items WHERE id = @Id";
            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<string> GetDistinctCategories()
        {
            return GetDistinctValues("category");
        }

        public List<string> GetDistinctDietaryStatuses()
        {
            return GetDistinctValues("dietary_status");
        }

        public List<string> GetDistinctStorageLocations()
        {
            return GetDistinctValues("storage_location");
        }

        private List<string> GetDistinctValues(string column)
        {
            List<string> values = new List<string>();
            string query = $"SELECT DISTINCT {column} FROM inventory_items WHERE {column} IS NOT NULL AND {column} != '' ORDER BY {column}";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    values.Add(reader.GetString(0));
            }
            return values;
        }

        private InventoryItem MapItem(MySqlDataReader reader)
        {
            return new InventoryItem
            {
                Id = reader.GetInt32("id"),
                ItemName = reader["item_name"].ToString(),
                Quantity = reader.GetInt32("quantity"),
                Unit = reader["unit"].ToString(),
                LowStockThreshold = reader.GetInt32("low_stock_threshold"),
                LastUpdated = reader.GetDateTime("last_updated"),
                Category = reader["category"] == DBNull.Value ? "" : reader["category"].ToString(),
                DietaryStatus = reader["dietary_status"] == DBNull.Value ? "" : reader["dietary_status"].ToString(),
                StorageLocation = reader["storage_location"] == DBNull.Value ? "" : reader["storage_location"].ToString(),
                UnitCost = reader["unit_cost"] == DBNull.Value ? 0 : reader.GetDecimal("unit_cost")
            };
        }

    }
}
