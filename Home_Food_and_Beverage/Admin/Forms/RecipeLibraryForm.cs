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
    public partial class RecipeLibraryForm : Form
    {

        private RecipeRepository recipeRepo = new RecipeRepository();

        public RecipeLibraryForm()
        {
            InitializeComponent();
        }

        private void RecipeLibraryForm_Load(object sender, EventArgs e)
        {
            LoadRecipes();
        }


        public void LoadRecipes()
        {
            LoadRecipes(txtSearch.Text.Trim());
        }

        private void LoadRecipes(string keyword)
        {
            panel3.Controls.Clear();

            var recipes = string.IsNullOrEmpty(keyword)
                ? recipeRepo.GetAllRecipes()
                : recipeRepo.SearchRecipes(keyword, publishedOnly: false);

            foreach (var recipe in recipes)
                panel3.Controls.Add(MakeCard(recipe));
        }

        private Panel MakeCard(Recipe recipe)
        {
            Panel card = new Panel
            {
                Size = new Size(panel3.Width - 25, 90),
                Margin = new Padding(2),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            PictureBox pic = new PictureBox
            {
                Location = new Point(5, 5),
                Size = new Size(80, 80),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Label lblName = new Label
            {
                Text = recipe.Name + (recipe.Status == "Draft" ? "  [Draft]" : ""),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(95, 8),
                AutoSize = true
            };

            Label lblDesc = new Label
            {
                Text = recipe.ServingNotes,
                Location = new Point(95, 32),
                Size = new Size(card.Width - 190, 45)
            };

            Button btnEdit = new Button
            {
                Text = "Edit",
                Location = new Point(card.Width - 90, 30),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            card.Controls.Add(pic);
            card.Controls.Add(lblName);
            card.Controls.Add(lblDesc);
            card.Controls.Add(btnEdit);
            return card;
        }

        private void OpenEditor(int? recipeId)
        {
            EditRecipeForm editor = new EditRecipeForm(recipeId, this);
            editor.Show();
            this.Hide();
        }


        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadRecipes(txtSearch.Text.Trim());
        }


        private void button6_Click(object sender, EventArgs e)
        {
            AdminDashboard dashboard = new AdminDashboard();
            dashboard.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenEditor(null);
        }
    }
}