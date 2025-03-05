using Microsoft.Data.SqlClient;
using ristorante_backend.Models;

namespace ristorante_backend.Repositories
{
    public class DishRepository
    {
        private const string connection_string = "Data Source=localhost;Initial Catalog=Ristorante;Integrated Security=True;Pooling=False;Encrypt=True;Trust Server Certificate=True";


        public void DishReader(SqlDataReader reader, Dictionary<int, Dish> dishes)
        {
            int id = reader.GetInt32(reader.GetOrdinal("id"));
            if (!dishes.TryGetValue(id, out Dish? dish))
            {
                dish = new Dish
                {
                    Id = id,
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                    Price = reader.GetDecimal(reader.GetOrdinal("price")),
                    CategoryId = reader.IsDBNull(reader.GetOrdinal("categoryId")) ? null : reader.GetInt32(reader.GetOrdinal("categoryId"))
                };
                dishes.Add(id, dish);

                if (!reader.IsDBNull(reader.GetOrdinal("categoryId")))
                {
                    dish.Category = new Category
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("categoryId")),
                        Name = reader.IsDBNull(reader.GetOrdinal("categoryName")) ? string.Empty : reader.GetString(reader.GetOrdinal("categoryName"))
                    };
                }
            }

            if (!reader.IsDBNull(reader.GetOrdinal("menuId")))
            {
                Menu menu = new Menu
                {
                    Id = reader.GetInt32(reader.GetOrdinal("menuId")),
                    Name = reader.IsDBNull(reader.GetOrdinal("menuName")) ? string.Empty : reader.GetString(reader.GetOrdinal("menuName"))
                };

                dish.Menus ??= new List<Menu>();
                if (!dish.Menus.Any(m => m.Id == menu.Id))
                {
                    dish.Menus.Add(menu);
                }
            }
        }


        public async Task<List<Dish>> GetDishes(int? limit = null)
        {
            using SqlConnection conn = new(connection_string);
            var dishes = new Dictionary<int, Dish>();

            string query = @$"SELECT {(limit == null ? "" : $"TOP {limit}")} 
                     D.*, C.Id as categoryId, C.Name as categoryName,
                     M.Id as menuId, M.Name as menuName
                     FROM Dishes D
                     LEFT JOIN Categories C ON D.CategoryId = C.Id
                     LEFT JOIN MenuDishes MD ON D.Id = MD.DishId
                     LEFT JOIN Menus M ON MD.MenuId = M.Id";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                DishReader(reader, dishes);
            }
            return dishes.Values.ToList();
        }


        public async Task<Dish?> GetDishById(int id)
        {
            using SqlConnection conn = new(connection_string);
            var dishes = new Dictionary<int, Dish>();

            string query = @"SELECT D.*, C.Id as categoryId, C.Name as categoryName,
                   M.Id as menuId, M.Name as menuName
                   FROM Dishes D
                   LEFT JOIN Categories C ON D.CategoryId = C.Id
                   LEFT JOIN MenuDishes MD ON D.Id = MD.DishId
                   LEFT JOIN Menus M ON MD.MenuId = M.Id
                   WHERE D.Id = @id";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                DishReader(reader, dishes);
            }
            return dishes.Values.FirstOrDefault();
        }

        public async Task<List<Dish>> GetDishesByCategory(int categoryId)
        {
            using SqlConnection conn = new(connection_string);
            var dishes = new Dictionary<int, Dish>();

            string query = @"SELECT D.*, C.Id as categoryId, C.Name as categoryName,
                           M.Id as menuId, M.Name as menuName
                           FROM Dish D
                           LEFT JOIN Category C ON D.CategoryId = C.Id
                           LEFT JOIN MenuDishes MD ON D.Id = MD.DishId
                           LEFT JOIN Menu M ON MD.MenuId = M.Id
                           WHERE D.CategoryId = @categoryId";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@categoryId", categoryId);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                DishReader(reader, dishes);
            }
            return dishes.Values.ToList();
        }

        public async Task<int> CreateDishAsync(Dish dish)
        {
            using SqlConnection conn = new(connection_string);
            string query = @"INSERT INTO Dishes (Name, Description, Price, CategoryId) 
                   VALUES (@name, @description, @price, @categoryId);
                   SELECT SCOPE_IDENTITY();";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@name", dish.Name);
            cmd.Parameters.AddWithValue("@description", dish.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@price", dish.Price);
            cmd.Parameters.AddWithValue("@categoryId", dish.CategoryId);

            int dishId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            dish.Id = dishId;

            return dishId;
        }


        public async Task<int> UpdateDish(int id, Dish dish)
        {
            using SqlConnection conn = new(connection_string);
            await conn.OpenAsync();

            // Prima verifico che la categoria esista (se è stata specificata)
            if (dish.CategoryId.HasValue)
            {
                string checkCategoryQuery = "SELECT COUNT(1) FROM Categories WHERE Id = @categoryId";
                using SqlCommand checkCmd = new(checkCategoryQuery, conn);
                checkCmd.Parameters.AddWithValue("@categoryId", dish.CategoryId);
                int categoryExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (categoryExists == 0)
                {
                    throw new InvalidOperationException($"La categoria con ID {dish.CategoryId} non esiste");
                }
            }

            // Se arrivo qui, la categoria esiste o è null
            string query = @"UPDATE Dishes 
                   SET Name = @name, 
                       Description = @description, 
                       Price = @price, 
                       CategoryId = @categoryId 
                   WHERE Id = @id";

            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", dish.Name);
            cmd.Parameters.AddWithValue("@description", dish.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@price", dish.Price);
            cmd.Parameters.AddWithValue("@categoryId", (object?)dish.CategoryId ?? DBNull.Value);

            return await cmd.ExecuteNonQueryAsync();
        }


        public async Task<int> DeleteDish(int id)
        {
            using SqlConnection conn = new(connection_string);

            // Prima rimuovo tutte le relazioni con i menù
            string deleteMenuDishesQuery = "DELETE FROM MenuDishes WHERE DishId = @id";
            await conn.OpenAsync();
            using (SqlCommand cmdMenuDishes = new(deleteMenuDishesQuery, conn))
            {
                cmdMenuDishes.Parameters.AddWithValue("@id", id);
                await cmdMenuDishes.ExecuteNonQueryAsync();
            }

            // Poi rimuovo il piatto
            string query = "DELETE FROM Dishes WHERE Id = @id";
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            return await cmd.ExecuteNonQueryAsync();
        }
    }
}
