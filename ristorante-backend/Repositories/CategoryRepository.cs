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
            // Modifica qui per gestire correttamente la nullabilità
            if (!categories.TryGetValue(id, out Category? category))
            {
                category = new Category
                {
                    Id = id,
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Dishes = new List<Dish>()  // Inizializza sempre la lista
                };
                categories.Add(id, category);
            }

            if (!reader.IsDBNull(reader.GetOrdinal("dishId")))
            {
                Dish dish = new()
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
                         FROM Categories C
                         LEFT JOIN Dishes D ON C.Id = D.CategoryId";
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
                       FROM Categories C
                       LEFT JOIN Dishes D ON C.Id = D.CategoryId
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
                       FROM Categories C
                       LEFT JOIN Dishes D ON C.Id = D.CategoryId
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
            string query = "INSERT INTO Categories (Name) VALUES (@name); SELECT SCOPE_IDENTITY();";
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
            string query = "UPDATE Categories SET Name = @name WHERE Id = @id";
            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", category.Name);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> DeleteCategory(int id)
        {
            using SqlConnection conn = new(connection_string);
            await conn.OpenAsync();

            using SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();

            try
            {
                // Aggiorna i piatti impostando CategoryId a NULL
                string updateDishesQuery = @"UPDATE Dishes 
                                   SET CategoryId = NULL 
                                   WHERE CategoryId = @id";

                using (SqlCommand updateCmd = new(updateDishesQuery, conn, transaction))
                {
                    updateCmd.Parameters.AddWithValue("@id", id);
                    await updateCmd.ExecuteNonQueryAsync();
                }

                // Elimina la categoria
                string deleteQuery = @"DELETE FROM Categories 
                             WHERE Id = @id";

                using SqlCommand deleteCmd = new(deleteQuery, conn, transaction);
                deleteCmd.Parameters.AddWithValue("@id", id);
                int result = await deleteCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                return result > 0;
            }
            catch (Exception)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                throw; 
            }
        }


        public async Task<bool> HasDishes(int categoryId)
        {
            using SqlConnection conn = new(connection_string);
            string query = "SELECT COUNT(*) FROM Dishes WHERE CategoryId = @categoryId AND CategoryId IS NOT NULL";
            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@categoryId", categoryId);

            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }
    }
}
