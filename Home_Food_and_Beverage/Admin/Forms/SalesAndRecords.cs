using Home_Food_and_Beverage.Data;
using Home_Food_and_Beverage.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Home_Food_and_Beverage.Admin.Forms
{
    public partial class SalesAndRecords : Form
    {
        private OrderRepository orderRepo = new OrderRepository();

        public SalesAndRecords()
        {
            InitializeComponent();
            
            cboCalendar.Items.AddRange(new string[] { "Today", "Yesterday", "Last 7 Days", "Last 30 Days", "This Month", "Last Month" });
            cboCalendar.SelectedIndex = 0;
            SetupGridColumns();
            LoadRecords();
        }
         
        private void SetupGridColumns()
        {
            dataGridView3.AutoGenerateColumns = false;
            dataGridView3.Columns.Clear();

            dataGridView3.Columns.Add(new DataGridViewTextBoxColumn { Name = "OrderDate", HeaderText = "Date", DataPropertyName = "OrderDate", DefaultCellStyle = new DataGridViewCellStyle { Format = "MM/dd/yyyy" } });
            dataGridView3.Columns.Add(new DataGridViewTextBoxColumn { Name = "PaymentMethod", HeaderText = "Order Type", DataPropertyName = "PaymentMethod" });
            dataGridView3.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemDetails", HeaderText = "Item Details", DataPropertyName = "ItemDetails", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dataGridView3.Columns.Add(new DataGridViewTextBoxColumn { Name = "StaffId", HeaderText = "Staff ID", DataPropertyName = "StaffId" });
            dataGridView3.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalAmount", HeaderText = "Total Sales", DataPropertyName = "TotalAmount", DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });

        }

        private void cboCalendar_SelectedIndexChanged(object sender, EventArgs e)
        {
         
        }

        private void LoadRecords()
        {
            string range = cboCalendar.SelectedItem?.ToString() ?? "Today";
            var orders = orderRepo.SearchOrders(range, textBox1.Text.Trim());

            var displayRows = orders.Select(o => new
            {
                o.Id,
                o.OrderDate,
                o.PaymentMethod,
                o.StaffId,
                o.TotalAmount,
                ItemDetails = orderRepo.GetItemSummary(o.Id)
            }).ToList();

            dataGridView3.DataSource = displayRows;

            UpdateStats(orders);
        }

        private void UpdateStats(System.Collections.Generic.List<Orders> orders)
        {
            // update primary payment display if available in designer
            if (orders.Count == 0)
            {
                if (lblMethod != null) lblMethod.Text = "N/A";
                return;
            }

            var mostCommon = orders
                .GroupBy(o => o.PaymentMethod)
                .OrderByDescending(g => g.Count())
                .First();
            if (lblMethod != null) lblMethod.Text = mostCommon.Key;
        }

        private void UpdateStats(System.Collections.Generic.List<Orders> orders, string range)
        {
            // Box 1 - "Selected Date": show the actual date(s) this range covers
            DateTime today = DateTime.Today;
            switch (range)
            {
                case "This Week":
                    DateTime weekStart = today.AddDays(-(int)today.DayOfWeek);
                    lblSelectedDateValue.Text = weekStart.ToString("MM/dd") + " - " + weekStart.AddDays(6).ToString("MM/dd/yyyy");
                    break;
                case "This Month":
                    lblSelectedDateValue.Text = today.ToString("MMMM yyyy");
                    break;
                default: // Today
                    lblSelectedDateValue.Text = today.ToString("MM/dd/yyyy");
                    break;
            }

            // Box 2 - "Total Sales"
            lblTotalSalesValue.Text = orders.Sum(o => o.TotalAmount).ToString("C2");

            // Box 3 - "Primary Payment"
            if (orders.Count == 0)
            {
                lblPrimaryPaymentValue.Text = "N/A";
                return;
            }

            var mostCommon = orders
                .GroupBy(o => o.PaymentMethod)
                .OrderByDescending(g => g.Count())
                .First();
            lblPrimaryPaymentValue.Text = mostCommon.Key;
        }


        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dataGridView3.Rows[e.RowIndex];
            int orderId = (int)row.Cells["Id"].Value;
            string columnName = dataGridView3.Columns[e.ColumnIndex].Name;

            if (columnName == "colView")
            {
                MessageBox.Show("TODO: show full order details for Order #" + orderId);
            }
            else if (columnName == "colEdit")
            {
                MessageBox.Show("TODO: open edit screen for Order #" + orderId);
            }
            else if (columnName == "colDelete")
            {
                var confirm = MessageBox.Show("Delete order #" + orderId + "?", "Confirm", MessageBoxButtons.YesNo);
                if (confirm == DialogResult.Yes)
                {
                    orderRepo.UpdateOrderStatus(orderId, "Cancelled");
                    LoadRecords();
                }

            }
        }


        private void button6_Click(object sender, EventArgs e)
        {
            AdminDashboard dashboard = new AdminDashboard();
            dashboard.Show();
            this.Hide();
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }
    }
}
 