using Home_Food_and_Beverage.Admin.Forms;
using Home_Food_and_Beverage.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Home_Food_and_Beverage.Admin
{
    public partial class AdminDashboard : Form
    {
        private RecipeRepository recipeRepo = new RecipeRepository();
        private InventoryRepository inventoryRepo = new InventoryRepository();
        private SalesRepository salesRepo = new SalesRepository();

        public AdminDashboard()
        {
            InitializeComponent();
        }

        private async void AdminDashboardForm_Load(object sender, EventArgs e)
        {
            lblActiveRecipes.Text = recipeRepo.GetAllRecipes().FindAll(r => r.Status == "Published").Count.ToString();
            lblLowStock.Text = inventoryRepo.GetLowStockCount().ToString();
            decimal todaySales = await salesRepo.GetRevenueAsync(DateTime.Now);
            lblTodaySales.Text = "₱" + todaySales.ToString("N2");
        }


        private void button5_Click_1(object sender, EventArgs e)
        {
            EmployeeManagement staff = new EmployeeManagement();
            staff.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SalesAndRecords reports = new SalesAndRecords();
            reports.Show();
            this.Hide();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            HeadAdminLogin dashboard = new HeadAdminLogin();
            dashboard.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            InventoryAndStockTracker inventory = new InventoryAndStockTracker();
            inventory.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RecipeLibraryForm recipeLibrary = new RecipeLibraryForm();
            recipeLibrary.Show();
            this.Hide();
        }
    }
}
