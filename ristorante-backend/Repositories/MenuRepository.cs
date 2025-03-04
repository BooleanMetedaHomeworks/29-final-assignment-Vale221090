using Microsoft.Data.SqlClient;
using ristorante_backend.Models;

namespace ristorante_backend.Repositories
{
    public class MenuRepository
    {
        private const string connection_string = "Data Source=localhost;Initial Catalog=Ristorante;Integrated Security=True;Trust Server Certificate=True";

        public void MenuReader(SqlDataReader reader, Dictionary<int, Menu> menus)
        {
            int id = reader.GetInt32(reader.GetOrdinal("id"));
            string name = reader.GetString(reader.GetOrdinal("name"));

            if (menus.TryGetValue(id, out Menu? menu) == false)
            {
                menu = new Menu { Name = name };
                menus.Add(id, menu);
                menu.Id = id;
            }

            if (reader.IsDBNull(reader.GetOrdinal("dishId")) == false)
            {
                Dish dish = new Dish
                {
                    Id = reader.GetInt32(reader.GetOrdinal("dishId")),
                    Name = reader.GetString(reader.GetOrdinal("dishName")),
                    Description = reader.IsDBNull(reader.GetOrdinal("dishDescription")) ? null : reader.GetString(reader.GetOrdinal("dishDescription")),
                    Price = reader.GetDecimal(reader.GetOrdinal("dishPrice")),
                    CategoryId = reader.GetInt32(reader.GetOrdinal("categoryId"))
                };

                if (reader.IsDBNull(reader.GetOrdinal("categoryId")) == false)
                {
                    dish.Category = new Category
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("categoryId")),
                        Name = reader.GetString(reader.GetOrdinal("categoryName"))
                    };
                }

                menu.Dishes ??= new List<Dish>();
                if (!menu.Dishes.Any(d => d.Id == dish.Id))
                {
                    menu.Dishes.Add(dish);
                }
            }
        }


        public async Task<List<Menu>> GetMenus(int? limit = null)
        {
            using SqlConnection conn = new(connection_string);
            var menus = new Dictionary<int, Menu>();

            string query = @$"SELECT {(limit == null ? "" : $"TOP {limit}")} 
                             M.*, D.Id as dishId, D.Name as dishName, 
                             D.Description as dishDescription, D.Price as dishPrice,
                             C.Id as categoryId, C.Name as categoryName
                             FROM Menu M
                             LEFT JOIN MenuDishes MD ON M.Id = MD.MenuId
                             LEFT JOIN Dish D ON MD.DishId = D.Id
                             LEFT JOIN Category C ON D.CategoryId = C.Id";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                MenuReader(reader, menus);
            }
            return menus.Values.ToList();
        }

        public async Task<Menu?> GetMenuById(int id)
        {
            using SqlConnection conn = new(connection_string);
            var menus = new Dictionary<int, Menu>();

            string query = @"SELECT M.*, D.Id as dishId, D.Name as dishName, 
                           D.Description as dishDescription, D.Price as dishPrice,
                           C.Id as categoryId, C.Name as categoryName
                           FROM Menu M
                           LEFT JOIN MenuDishes MD ON M.Id = MD.MenuId
                           LEFT JOIN Dish D ON MD.DishId = D.Id
                           LEFT JOIN Category C ON D.CategoryId = C.Id
                           WHERE M.Id = @id";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                MenuReader(reader, menus);
            }
            return menus.Values.FirstOrDefault();
        }

        public async Task<int> CreateMenuAsync(Menu menu)
        {
            using SqlConnection conn = new(connection_string);
            string query = "INSERT INTO Menu (Name) VALUES (@name); SELECT SCOPE_IDENTITY();";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@name", menu.Name);

            int menuId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            menu.Id = menuId;

            if (menu.Dishes?.Any() == true)
            {
                await GestisciPiatti(menu.Dishes.Select(d => d.Id).ToList(), menuId, conn);
            }

            return menuId;
        }

        public async Task<int> UpdateMenu(int id, Menu menu)
        {
            using SqlConnection conn = new(connection_string);
            string query = "UPDATE Menu SET Name = @name WHERE Id = @id";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", menu.Name);

            int affectedRows = await cmd.ExecuteNonQueryAsync();

            if (menu.Dishes != null)
            {
                await GestisciPiatti(menu.Dishes.Select(d => d.Id).ToList(), id, conn);
            }

            return affectedRows;
        }

        public async Task<int> DeleteMenu(int id)
        {
            using SqlConnection conn = new(connection_string);
            string query = "DELETE FROM Menu WHERE Id = @id";

            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            return await cmd.ExecuteNonQueryAsync();
        }

        private async Task<int> ClearMenuDishes(int menuId, SqlConnection conn)
        {
            string query = "DELETE FROM MenuDishes WHERE MenuId = @menuId";
            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@menuId", menuId);
            return await cmd.ExecuteNonQueryAsync();
        }

        private async Task<int> AddMenuDishes(int menuId, List<int> dishIds, SqlConnection conn)
        {
            int counter = 0;
            string query = "INSERT INTO MenuDishes (MenuId, DishId) VALUES (@menuId, @dishId)";

            foreach (int dishId in dishIds)
            {
                using SqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@menuId", menuId);
                cmd.Parameters.AddWithValue("@dishId", dishId);
                counter += await cmd.ExecuteNonQueryAsync();
            }
            return counter;
        }

        private async Task GestisciPiatti(List<int> dishIds, int menuId, SqlConnection conn)
        {
            if (dishIds == null)
                return;

            await ClearMenuDishes(menuId, conn);
            await AddMenuDishes(menuId, dishIds, conn);
        }
    }
}
