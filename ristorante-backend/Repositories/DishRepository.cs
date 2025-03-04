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
            if (dishes.TryGetValue(id, out Dish dish) == false)
            {
                dish = new Dish
                {
                    Id = id,
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                    Price = reader.GetDecimal(reader.GetOrdinal("price")),
                    CategoryId = reader.GetInt32(reader.GetOrdinal("categoryId"))
                };
                dishes.Add(id, dish);

                if (reader.IsDBNull(reader.GetOrdinal("categoryId")) == false)
                {
                    dish.Category = new Category
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("categoryId")),
                        Name = reader.GetString(reader.GetOrdinal("categoryName"))
                    };
                }
            }

            if (reader.IsDBNull(reader.GetOrdinal("menuId")) == false)
            {
                Menu menu = new Menu
                {
                    Id = reader.GetInt32(reader.GetOrdinal("menuId")),
                    Name = reader.GetString(reader.GetOrdinal("menuName"))
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
                             FROM Dish D
                             LEFT JOIN Category C ON D.CategoryId = C.Id
                             LEFT JOIN MenuDishes MD ON D.Id = MD.DishId
                             LEFT JOIN Menu M ON MD.MenuId = M.Id";

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
                           FROM Dish D
                           LEFT JOIN Category C ON D.CategoryId = C.Id
                           LEFT JOIN MenuDishes MD ON D.Id = MD.DishId
                           LEFT JOIN Menu M ON MD.MenuId = M.Id
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
            string query = @"INSERT INTO Dish (Name, Description, Price, CategoryId) 
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
            string query = @"UPDATE Dish 
                           SET Name = @name, 
                               Description = @description, 
                               Price = @price, 
                               CategoryId = @categoryId 
                           WHERE Id = @id";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", dish.Name);
            cmd.Parameters.AddWithValue("@description", dish.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@price", dish.Price);
            cmd.Parameters.AddWithValue("@categoryId", dish.CategoryId);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> DeleteDish(int id)
        {
            using SqlConnection conn = new(connection_string);

            // Prima rimuovi tutte le relazioni con i menù
            string deleteMenuDishesQuery = "DELETE FROM MenuDishes WHERE DishId = @id";
            await conn.OpenAsync();
            using (SqlCommand cmdMenuDishes = new(deleteMenuDishesQuery, conn))
            {
                cmdMenuDishes.Parameters.AddWithValue("@id", id);
                await cmdMenuDishes.ExecuteNonQueryAsync();
            }

            // Poi rimuovi il piatto
            string query = "DELETE FROM Dish WHERE Id = @id";
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            return await cmd.ExecuteNonQueryAsync();
        }
    }
}
