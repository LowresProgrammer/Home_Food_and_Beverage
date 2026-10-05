using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home_Food_and_Beverage.Models
{
    internal class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PortionSize { get; set; }
        public string PrepSteps { get; set; }
        public string ServingNotes { get; set; }
        public string AllergenTags { get; set; }
        public string ServeTemp { get; set; }      // "Hot" / "Cold"
        public string ImagePath { get; set; }
        public string Status { get; set; }         // "Draft" / "Published"
        public List<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
    }

    public class RecipeIngredient
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public string IngredientName { get; set; }
        public string Quantity { get; set; }
    }

}
