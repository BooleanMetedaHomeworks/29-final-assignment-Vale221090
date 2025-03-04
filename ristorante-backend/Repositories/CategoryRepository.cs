using Microsoft.Data.SqlClient;
using ristorante_backend.Models;

namespace ristorante_backend.Repositories
{
    public class CategoryRepository
    {
        private const string connection_string = "Data Source=localhost;Initial Catalog=Ristorante;Integrated Security=True;Trust Server Certificate=True";

        public void CategoryReader(SqlDataReader reader, Dictionary<int, Category> categories)
        {
            int id = reader.GetInt32(reader.GetOrdinal("id"));
            if (categories.TryGetValue(id, out Category category) == false)
            {
                category = new Category
                {
                    Id = id,
                    Name = reader.GetString(reader.GetOrdinal("name"))
                };
                categories.Add(id, category);
            }

            if (reader.IsDBNull(reader.GetOrdinal("dishId")) == false)
            {
                Dish dish = new Dish
                {
                    Id = reader.GetInt32(reader.GetOrdinal("dishId")),
                    Name = reader.GetString(reader.GetOrdinal("dishName")),
                    Description = reader.IsDBNull(reader.GetOrdinal("dishDescription")) ? null : reader.GetString(reader.GetOrdinal("dishDescription")),
                    Price = reader.GetDecimal(reader.GetOrdinal("dishPrice")),
                    CategoryId = category.Id,
                    Category = category
                };

                category.Dishes ??= new List<Dish>();
                if (!category.Dishes.Any(d => d.Id == dish.Id))
                {
                    category.Dishes.Add(dish);
                }
            }
        }

        public async Task<List<Category>> GetCategories(int? limit = null)
        {
            using SqlConnection conn = new(connection_string);
            var categories = new Dictionary<int, Category>();

            string query = @$"SELECT {(limit == null ? "" : $"TOP {limit}")} 
                             C.*, D.Id as dishId, D.Name as dishName,
                             D.Description as dishDescription, D.Price as dishPrice
                             FROM Category C
                             LEFT JOIN Dish D ON C.Id = D.CategoryId";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                CategoryReader(reader, categories);
            }
            return categories.Values.ToList();
        }

        public async Task<Category?> GetCategoryById(int id)
        {
            using SqlConnection conn = new(connection_string);
            var categories = new Dictionary<int, Category>();

            string query = @"SELECT C.*, D.Id as dishId, D.Name as dishName,
                           D.Description as dishDescription, D.Price as dishPrice
                           FROM Category C
                           LEFT JOIN Dish D ON C.Id = D.CategoryId
                           WHERE C.Id = @id";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                CategoryReader(reader, categories);
            }
            return categories.Values.FirstOrDefault();
        }

        public async Task<List<Category>> GetCategoriesWithDishes()
        {
            using SqlConnection conn = new(connection_string);
            var categories = new Dictionary<int, Category>();

            string query = @"SELECT C.*, D.Id as dishId, D.Name as dishName,
                           D.Description as dishDescription, D.Price as dishPrice
                           FROM Category C
                           LEFT JOIN Dish D ON C.Id = D.CategoryId
                           ORDER BY C.Name";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                CategoryReader(reader, categories);
            }
            return categories.Values.ToList();
        }

        public async Task<int> CreateCategoryAsync(Category category)
        {
            using SqlConnection conn = new(connection_string);
            string query = "INSERT INTO Category (Name) VALUES (@name); SELECT SCOPE_IDENTITY();";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@name", category.Name);

            int categoryId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            category.Id = categoryId;

            return categoryId;
        }

        public async Task<int> UpdateCategory(int id, Category category)
        {
            using SqlConnection conn = new(connection_string);
            string query = "UPDATE Category SET Name = @name WHERE Id = @id";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", category.Name);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> DeleteCategory(int id)
        {
            using SqlConnection conn = new(connection_string);

            // Verifica se ci sono piatti associati
            string checkQuery = "SELECT COUNT(*) FROM Dish WHERE CategoryId = @id";
            await conn.OpenAsync();
            using (SqlCommand checkCmd = new(checkQuery, conn))
            {
                checkCmd.Parameters.AddWithValue("@id", id);
                int dishCount = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                if (dishCount > 0)
                {
                    return false; // Non possiamo eliminare una categoria con piatti associati
                }
            }

            // Se non ci sono piatti, procedi con l'eliminazione
            string deleteQuery = "DELETE FROM Category WHERE Id = @id";
            using SqlCommand deleteCmd = new(deleteQuery, conn);
            deleteCmd.Parameters.AddWithValue("@id", id);
            int result = await deleteCmd.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> HasDishes(int categoryId)
        {
            using SqlConnection conn = new(connection_string);
            string query = "SELECT COUNT(*) FROM Dish WHERE CategoryId = @categoryId";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@categoryId", categoryId);

            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }
    }
}
