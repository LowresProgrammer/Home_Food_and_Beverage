using Home_Food_and_Beverage.Data;
using Home_Food_and_Beverage.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Home_Food_and_Beverage.Admin.Forms
{
    public partial class InventoryAndStockTracker : Form
    {
        private InventoryRepository inventoryRepo = new InventoryRepository();


        public InventoryAndStockTracker()
        {
            InitializeComponent();
            LoadFilterOptions();
            SetupGridColumns();
            RefreshAll();

        }

        private void LoadFilterOptions()
        {
            cboCategory.Items.Clear();
            cboCategory.Items.Add("All");
            cboCategory.Items.AddRange(inventoryRepo.GetDistinctCategories().ToArray());
            cboCategory.SelectedIndex = 0;

            cboDietaryStatus.Items.Clear();
            cboDietaryStatus.Items.Add("All");
            cboDietaryStatus.Items.AddRange(inventoryRepo.GetDistinctDietaryStatuses().ToArray());
            cboDietaryStatus.SelectedIndex = 0;

            cboStorageLocation.Items.Clear();
            cboStorageLocation.Items.Add("All");
            cboStorageLocation.Items.AddRange(inventoryRepo.GetDistinctStorageLocations().ToArray());
            cboStorageLocation.SelectedIndex = 0;

            cboDateRange.Items.Clear();
            cboDateRange.Items.AddRange(new object[] { "All", "Today", "This Week", "This Month" });
            cboDateRange.SelectedIndex = 0;
        }

        private void SetupGridColumns()
        {
            dataGridView3.AutoGenerateColumns = false;
            dataGridView3.Columns.Clear();
            dataGridView3.ReadOnly = true;
            dataGridView3.AllowUserToAddRows = false;

            dataGridView3.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemName", HeaderText = "Item", DataPropertyName = "ItemName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dataGridView3.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category", DataPropertyName = "Category" });
            dataGridView3.Columns.Add(new DataGridViewTextBoxColumn { Name = "DietaryStatus", HeaderText = "Dietary Status", DataPropertyName = "DietaryStatus" });
            dataGridView3.Columns.Add(new DataGridViewTextBoxColumn { Name = "StorageLocation", HeaderText = "Storage", DataPropertyName = "StorageLocation" });
            dataGridView3.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Qty", DataPropertyName = "Quantity" });
            dataGridView3.Columns.Add(new DataGridViewTextBoxColumn { Name = "Unit", HeaderText = "Unit", DataPropertyName = "Unit" });
            dataGridView3.Columns.Add(new DataGridViewTextBoxColumn { Name = "StockValue", HeaderText = "Value", DataPropertyName = "StockValue", DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
        }

        private void RefreshAll()
        {
            LoadStats();
            LoadGrid();
            LoadLowStockAlerts();
        }

        private void LoadStats()
        {
            lblTotalIngredientsValue.Text = inventoryRepo.GetTotalIngredientCount().ToString("N0");
            lblStockValueValue.Text = "₱" + inventoryRepo.GetTotalStockValue().ToString("N2");
            lblLowStockValue.Text = inventoryRepo.GetLowStockCount().ToString();
        }

        private void LoadGrid()
        {
            string category = cboCategory.SelectedItem?.ToString() ?? "All";
            string dietary = cboDietaryStatus.SelectedItem?.ToString() ?? "All";
            string storage = cboStorageLocation.SelectedItem?.ToString() ?? "All";
            string dateRange = cboDateRange.SelectedItem?.ToString() ?? "All";

            var items = inventoryRepo.SearchItems(category, dietary, storage, dateRange, txtSearch.Text.Trim());
            dataGridView3.DataSource = items;
        }

        private void LoadLowStockAlerts()
        {
            lowStockPanel.Controls.Clear();
            foreach (InventoryItem item in inventoryRepo.GetLowStockItems())
                lowStockPanel.Controls.Add(MakeLowStockRow(item));
        }

        private Panel MakeLowStockRow(InventoryItem item)
        {
            Panel row = new Panel { Size = new Size(300, 32), Margin = new Padding(2) };

            Label lblItem = new Label
            {
                Text = "✓ " + item.ItemName,
                ForeColor = Color.White,
                Location = new Point(0, 6),
                AutoSize = true
            };

            Button btnReorder = new Button
            {
                Text = "Reorder",
                Location = new Point(180, 2),
                Size = new Size(103, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White
            };
            btnReorder.Click += (s, e) => ReorderItem(item);

            row.Controls.Add(lblItem);
            row.Controls.Add(btnReorder);
            return row;
        }

        private void ReorderItem(InventoryItem item)
        {
            string input = ShowInputBox($"How many {item.Unit} of {item.ItemName} to receive?", "Reorder");
            if (!int.TryParse(input, out int qty) || qty <= 0) return;

            inventoryRepo.ReceiveStock(item.Id, qty);
            MessageBox.Show($"Received {qty} {item.Unit} of {item.ItemName}.");
            RefreshAll();
        }


        private void button6_Click(object sender, EventArgs e)
        {
            AdminDashboard dashboard = new AdminDashboard();
            dashboard.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string name = ShowInputBox("Item name:", "Add New Item");
            if (string.IsNullOrWhiteSpace(name)) return;

            string qtyText = ShowInputBox("Starting quantity:", "Add New Item");
            if (!int.TryParse(qtyText, out int qty)) return;

            string unit = ShowInputBox("Unit (kg, pcs, liters...):", "Add New Item");
            string category = ShowInputBox("Category:", "Add New Item");
            string dietary = ShowInputBox("Dietary Status (e.g. Vegetarian, Contains Nuts, None):", "Add New Item");
            string storage = ShowInputBox("Storage Location (e.g. Fridge, Freezer, Dry Storage):", "Add New Item");

            string costText = ShowInputBox("Unit cost (₱):", "Add New Item");
            decimal.TryParse(costText, out decimal cost);

            string thresholdText = ShowInputBox("Low stock threshold:", "Add New Item", "10");
            int.TryParse(thresholdText, out int threshold);
            if (threshold <= 0) threshold = 10;

            inventoryRepo.AddItem(new InventoryItem
            {
                ItemName = name,
                Quantity = qty,
                Unit = unit,
                Category = category,
                DietaryStatus = dietary,
                StorageLocation = storage,
                UnitCost = cost,
                LowStockThreshold = threshold
            });

            LoadFilterOptions();
            RefreshAll();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView3.CurrentRow == null || !(dataGridView3.CurrentRow.DataBoundItem is InventoryItem selected))
            {
                MessageBox.Show("Select an item in the list first.");
                return;
            }

            ReorderItem(selected);

        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (dataGridView3.CurrentRow == null || !(dataGridView3.CurrentRow.DataBoundItem is InventoryItem selected))
            {
                MessageBox.Show("Select a stock item from the list first.");
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Delete stock item " + selected.ItemName + "?", "Confirm delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                inventoryRepo.DeleteItem(selected.Id);
            }
            catch (MySql.Data.MySqlClient.MySqlException ex) when (ex.Number == 1451)
            {
                MessageBox.Show(
                    "This stock item can't be deleted because it's used in one or more recipes.",
                    "Cannot delete");
                return;
            }

            MessageBox.Show("Stock item deleted.");
            RefreshAll();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TODO: link this to Order Taking, or a purchase-order screen for suppliers.");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv",
                FileName = "inventory_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv"
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;

            var items = inventoryRepo.GetAllItems();
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("Item,Category,DietaryStatus,Storage,Quantity,Unit,UnitCost,StockValue");

            foreach (var i in items)
                csv.AppendLine($"{Escape(i.ItemName)},{Escape(i.Category)},{Escape(i.DietaryStatus)},{Escape(i.StorageLocation)},{i.Quantity},{Escape(i.Unit)},{i.UnitCost},{i.StockValue}");

            File.WriteAllText(dialog.FileName, csv.ToString(), Encoding.UTF8);
            MessageBox.Show($"Saved {items.Count} items.");

        }

        private string Escape(string value)
        {
            return "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
        }

        private string ShowInputBox(string prompt, string title, string defaultValue = "")
        {
            using (Form form = new Form())
            {
                form.Text = title;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.ClientSize = new Size(420, 120);

                Label label = new Label() { Left = 10, Top = 10, Width = 400, Text = prompt };
                TextBox textBox = new TextBox() { Left = 10, Top = 35, Width = 390, Text = defaultValue };
                Button ok = new Button() { Text = "OK", Left = 230, Width = 80, Top = 70, DialogResult = DialogResult.OK };
                Button cancel = new Button() { Text = "Cancel", Left = 320, Width = 80, Top = 70, DialogResult = DialogResult.Cancel };

                form.Controls.Add(label);
                form.Controls.Add(textBox);
                form.Controls.Add(ok);
                form.Controls.Add(cancel);

                form.AcceptButton = ok;
                form.CancelButton = cancel;

                return form.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
            }
        }

        // ---------- Filter events ----------

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e) => LoadGrid();
        private void cboDietaryStatus_SelectedIndexChanged(object sender, EventArgs e) => LoadGrid();
        private void cboStorageLocation_SelectedIndexChanged(object sender, EventArgs e) => LoadGrid();
        private void cboDateRange_SelectedIndexChanged(object sender, EventArgs e) => LoadGrid();
        private void txtSearch_TextChanged(object sender, EventArgs e) => LoadGrid();

    }
}

