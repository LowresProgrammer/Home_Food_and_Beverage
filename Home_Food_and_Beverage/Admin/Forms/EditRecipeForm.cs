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
    public partial class EditRecipeForm : Form
    {
        private RecipeRepository recipeRepo = new RecipeRepository();
        private Recipe currentRecipe;
        private int? recipeId;
        private RecipeLibraryForm parentLibrary;



        public EditRecipeForm(int? recipeId, RecipeLibraryForm recipeLibraryForm)
        {
            InitializeComponent();
            InitializeComponent();
            this.recipeId = recipeId;
            this.parentLibrary = parentLibrary;

            SetPlaceholder(txtName, "e.g. Iced Spanish Latte");
            SetPlaceholder(txtPortionSize, "e.g. 12 oz");
            SetPlaceholder(txtPrepSteps, "Step-by-step instructions...");
            SetPlaceholder(txtServingNotes, "e.g. served hot");
            SetPlaceholder(txtAllergenTags, "e.g. nuts, dairy");

            if (recipeId.HasValue)
            {
                currentRecipe = recipeRepo.GetRecipeById(recipeId.Value);
                LoadIntoForm(currentRecipe);
            }
            else
            {
                currentRecipe = new Recipe { Status = "Draft" };
                lblStatus.Text = "Draft";
                AddIngredientRow("", "");
                AddIngredientRow("", "");
            }

        }

        private void LoadIntoForm(Recipe recipe)
        {
            SetFieldValue(txtName, recipe.Name);
            SetFieldValue(txtPortionSize, recipe.PortionSize);
            SetFieldValue(txtPrepSteps, recipe.PrepSteps);
            SetFieldValue(txtServingNotes, recipe.ServingNotes);
            SetFieldValue(txtAllergenTags, recipe.AllergenTags);
            lblStatus.Text = recipe.Status;

            ingredientsPanel.Controls.Clear();
            if (recipe.Ingredients.Count == 0)
            {
                AddIngredientRow("", "");
                AddIngredientRow("", "");
            }
            else
            {
                foreach (var ing in recipe.Ingredients)
                    AddIngredientRow(ing.IngredientName, ing.Quantity);
            }

        }


        private void button6_Click(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Save(publish: false);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Save(publish: true);
        }

        private void AddIngredientRow(string name, string qty)
        {
            Panel row = new Panel { Size = new Size(ingredientsPanel.Width - 20, 30) };

            TextBox txtIngName = new TextBox { Location = new Point(0, 2), Width = 280, Tag = "name" };
            TextBox txtIngQty = new TextBox { Location = new Point(290, 2), Width = 120, Tag = "qty" };

            SetPlaceholder(txtIngName, "Ingredient");
            SetPlaceholder(txtIngQty, "Qty");
            SetFieldValue(txtIngName, name);
            SetFieldValue(txtIngQty, qty);

            Button btnAdd = new Button
            {
                Text = "+",
                Location = new Point(420, 0),
                Size = new Size(40, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            btnAdd.Click += (s, e) => AddIngredientRow("", "");

            row.Controls.Add(txtIngName);
            row.Controls.Add(txtIngQty);
            row.Controls.Add(btnAdd);
            ingredientsPanel.Controls.Add(row);
        }

        private void btnSaveDraft_Click(object sender, EventArgs e)
        {
            Save(publish: false);
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            Save(publish: true);
        }

        private void Save(bool publish)
        {
            if (string.IsNullOrWhiteSpace(GetFieldValue(txtName)))
            {
                MessageBox.Show("Recipe name is required.");
                return;
            }

            currentRecipe.Name = GetFieldValue(txtName);
            currentRecipe.PortionSize = GetFieldValue(txtPortionSize);
            currentRecipe.PrepSteps = GetFieldValue(txtPrepSteps);
            currentRecipe.ServingNotes = GetFieldValue(txtServingNotes);
            currentRecipe.AllergenTags = GetFieldValue(txtAllergenTags);

            currentRecipe.Ingredients.Clear();
            foreach (Panel row in ingredientsPanel.Controls)
            {
                TextBox tName = null, tQty = null;
                foreach (Control c in row.Controls)
                {
                    if (c is TextBox tb)
                    {
                        if ((string)tb.Tag == "name") tName = tb;
                        if ((string)tb.Tag == "qty") tQty = tb;
                    }
                }

                string ingName = GetFieldValue(tName);
                if (tName != null && !string.IsNullOrWhiteSpace(ingName))
                {
                    currentRecipe.Ingredients.Add(new RecipeIngredient
                    {
                        IngredientName = ingName,
                        Quantity = GetFieldValue(tQty)
                    });
                }
            }

            if (publish)
            {
                currentRecipe.Id = recipeRepo.PublishRecipe(currentRecipe);
                lblStatus.Text = "Published";
                MessageBox.Show("Recipe published! Staff can now see this.");
            }
            else
            {
                currentRecipe.Id = recipeRepo.SaveAsDraft(currentRecipe);
                lblStatus.Text = "Draft";
                MessageBox.Show("Saved as draft. Staff will not see this until published.");
            }


        }



        private void SetPlaceholder(TextBox box, string placeholderText)
        {
            box.ForeColor = Color.Gray;
            box.Font = new Font(box.Font, FontStyle.Italic);
            box.Text = placeholderText;
            box.Tag = box.Tag ?? "";
            string realTag = box.Tag.ToString();

            box.Enter += (s, e) =>
            {
                if (box.ForeColor == Color.Gray && box.Text == placeholderText)
                {
                    box.Text = "";
                    box.ForeColor = Color.Black;
                    box.Font = new Font(box.Font, FontStyle.Regular);
                }
            };

            box.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(box.Text))
                {
                    box.Text = placeholderText;
                    box.ForeColor = Color.Gray;
                    box.Font = new Font(box.Font, FontStyle.Italic);
                }
            };

            box.Tag = new PlaceholderInfo { OriginalTag = realTag, PlaceholderText = placeholderText };
        }

        private string GetFieldValue(TextBox box)
        {
            if (box == null) return "";
            if (box.Tag is PlaceholderInfo info && box.Text == info.PlaceholderText && box.ForeColor == Color.Gray)
                return "";
            return box.Text;
        }

        private void SetFieldValue(TextBox box, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                box.Text = value;
                box.ForeColor = Color.Black;
                box.Font = new Font(box.Font, FontStyle.Regular);
            }
        }

        private class PlaceholderInfo
        {
            public string OriginalTag;
            public string PlaceholderText;
        }


        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
