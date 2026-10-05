using Home_Food_and_Beverage.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home_Food_and_Beverage.Data
{
    internal class RecipeRepository
    {
        public List<Recipe> GetPublishedRecipes()
        {
            return GetRecipes("WHERE status = 'Published'");
        }

        public List<Recipe> GetAllRecipes()
        {
            return GetRecipes(null);
        }

        public List<Recipe> SearchRecipes(string keyword, bool publishedOnly)
        {
            string where = publishedOnly
                ? "WHERE status = 'Published' AND name LIKE @Keyword"
                : "WHERE name LIKE @Keyword";

            List<Recipe> recipes = new List<Recipe>();
            string query = $"SELECT * FROM recipes {where}";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");
                conn.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        recipes.Add(MapRecipe(reader));
                }
            }
            return recipes;
        }

        public Recipe GetRecipeById(int id)
        {
            Recipe recipe = null;
            string query = "SELECT * FROM recipes WHERE id = @Id";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        recipe = MapRecipe(reader);
                }
            }

            if (recipe != null)
                recipe.Ingredients = GetIngredientsForRecipe(recipe.Id);

            return recipe;
        }

        public List<RecipeIngredient> GetIngredientsForRecipe(int recipeId)
        {
            List<RecipeIngredient> list = new List<RecipeIngredient>();
            string query = "SELECT id, recipe_id, ingredient_name, quantity FROM recipe_ingredients WHERE recipe_id = @RecipeId";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@RecipeId", recipeId);
                conn.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new RecipeIngredient
                        {
                            Id = reader.GetInt32("id"),
                            RecipeId = reader.GetInt32("recipe_id"),
                            IngredientName = reader.GetString("ingredient_name"),
                            Quantity = reader.GetString("quantity")
                        });
                    }
                }
            }
            return list;
        }

        public int SaveAsDraft(Recipe recipe)
        {
            return InsertOrUpdate(recipe, "Draft");
        }

        public int PublishRecipe(Recipe recipe)
        {
            return InsertOrUpdate(recipe, "Published");
        }

        public void RetireRecipe(int id)
        {
            string deleteIngredients = "DELETE FROM recipe_ingredients WHERE recipe_id = @Id";
            string deleteRecipe = "DELETE FROM recipes WHERE id = @Id";

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(deleteIngredients, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
                using (MySqlCommand cmd = new MySqlCommand(deleteRecipe, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private List<Recipe> GetRecipes(string whereClause)
        {
            List<Recipe> recipes = new List<Recipe>();
            string query = "SELECT * FROM recipes " + (whereClause ?? "");

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        recipes.Add(MapRecipe(reader));
                }
            }
            return recipes;
        }

        private int InsertOrUpdate(Recipe recipe, string status)
        {
            int recipeId;
            string query;

            if (recipe.Id == 0)
            {
                query = @"INSERT INTO recipes (name, portion_size, prep_steps, serving_notes, allergen_tags, serve_temp, status)
                          VALUES (@Name, @PortionSize, @PrepSteps, @ServingNotes, @AllergenTags, @ServeTemp, @Status);
                          SELECT LAST_INSERT_ID();";
            }
            else
            {
                query = @"UPDATE recipes SET name=@Name, portion_size=@PortionSize, prep_steps=@PrepSteps,
                          serving_notes=@ServingNotes, allergen_tags=@AllergenTags, serve_temp=@ServeTemp, status=@Status
                          WHERE id=@Id";
            }

            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (MySqlTransaction tx = conn.BeginTransaction())
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@Id", recipe.Id);
                        cmd.Parameters.AddWithValue("@Name", recipe.Name);
                        cmd.Parameters.AddWithValue("@PortionSize", recipe.PortionSize);
                        cmd.Parameters.AddWithValue("@PrepSteps", recipe.PrepSteps);
                        cmd.Parameters.AddWithValue("@ServingNotes", recipe.ServingNotes);
                        cmd.Parameters.AddWithValue("@AllergenTags", recipe.AllergenTags);
                        cmd.Parameters.AddWithValue("@ServeTemp", recipe.ServeTemp ?? "");
                        cmd.Parameters.AddWithValue("@Status", status);

                        if (recipe.Id == 0)
                        {
                            object res = cmd.ExecuteScalar();
                            recipeId = Convert.ToInt32(res);
                        }
                        else
                        {
                            cmd.ExecuteNonQuery();
                            recipeId = recipe.Id;
                        }
                    }

                    ReplaceIngredients(recipeId, recipe.Ingredients, conn, tx);

                    tx.Commit();
                }
            }

            return recipeId;
        }

        private void ReplaceIngredients(int recipeId, List<RecipeIngredient> ingredients)
        {
            using (MySqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (MySqlTransaction tx = conn.BeginTransaction())
                {
                    ReplaceIngredients(recipeId, ingredients, conn, tx);
                    tx.Commit();
                }
            }
        }

        private void ReplaceIngredients(int recipeId, List<RecipeIngredient> ingredients, MySqlConnection conn, MySqlTransaction tx)
        {
            using (MySqlCommand del = new MySqlCommand("DELETE FROM recipe_ingredients WHERE recipe_id = @RecipeId", conn, tx))
            {
                del.Parameters.AddWithValue("@RecipeId", recipeId);
                del.ExecuteNonQuery();
            }

            if (ingredients == null || ingredients.Count == 0)
                return;

            using (MySqlCommand ins = new MySqlCommand("INSERT INTO recipe_ingredients (recipe_id, ingredient_name, quantity) VALUES (@RecipeId, @IngredientName, @Quantity)", conn, tx))
            {
                ins.Parameters.Add(new MySqlParameter("@RecipeId", recipeId));
                ins.Parameters.Add(new MySqlParameter("@IngredientName", ""));
                ins.Parameters.Add(new MySqlParameter("@Quantity", ""));

                foreach (var ingredient in ingredients)
                {
                    ins.Parameters["@RecipeId"].Value = recipeId;
                    ins.Parameters["@IngredientName"].Value = ingredient.IngredientName ?? "";
                    ins.Parameters["@Quantity"].Value = ingredient.Quantity ?? "";
                    ins.ExecuteNonQuery();
                }
            }
        }

        private Recipe MapRecipe(MySqlDataReader reader)
        {
            return new Recipe
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                PortionSize = reader.IsDBNull(reader.GetOrdinal("portion_size")) ? "" : reader.GetString("portion_size"),
                PrepSteps = reader.IsDBNull(reader.GetOrdinal("prep_steps")) ? "" : reader.GetString("prep_steps"),
                ServingNotes = reader.IsDBNull(reader.GetOrdinal("serving_notes")) ? "" : reader.GetString("serving_notes"),
                AllergenTags = reader.IsDBNull(reader.GetOrdinal("allergen_tags")) ? "" : reader.GetString("allergen_tags"),
                ServeTemp = reader.IsDBNull(reader.GetOrdinal("serve_temp")) ? "" : reader.GetString("serve_temp"),
                ImagePath = reader.IsDBNull(reader.GetOrdinal("image_path")) ? "" : reader.GetString("image_path"),
                Status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString("status")
            };
        }
    }
}
