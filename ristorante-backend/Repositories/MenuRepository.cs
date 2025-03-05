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
                     FROM Menus M
                     LEFT JOIN MenuDishes MD ON M.Id = MD.MenuId
                     LEFT JOIN Dishes D ON MD.DishId = D.Id
                     LEFT JOIN Categories C ON D.CategoryId = C.Id";

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
                   FROM Menus M
                   LEFT JOIN MenuDishes MD ON M.Id = MD.MenuId
                   LEFT JOIN Dishes D ON MD.DishId = D.Id
                   LEFT JOIN Categories C ON D.CategoryId = C.Id
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
            await conn.OpenAsync();

            using SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();

            try
            {
                
                string query = "INSERT INTO Menus (Name) VALUES (@name); SELECT SCOPE_IDENTITY();";
                using SqlCommand cmd = new(query, conn, transaction);
                cmd.Parameters.AddWithValue("@name", menu.Name);

                int menuId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                menu.Id = menuId;

                // Se ci sono piatti, verifico che esistano tutti prima di inserirli
                if (menu.Dishes?.Any() == true)
                {
                    var dishIds = menu.Dishes.Select(d => d.Id).ToList();
                    string verifyDishesQuery = @"
                SELECT COUNT(*) 
                FROM Dishes 
                WHERE Id IN (" + string.Join(",", dishIds) + ")";

                    using SqlCommand verifyCmd = new(verifyDishesQuery, conn, transaction);
                    int existingDishCount = Convert.ToInt32(await verifyCmd.ExecuteScalarAsync());

                    if (existingDishCount != dishIds.Count)
                    {
                        throw new InvalidOperationException("Uno o più piatti specificati non esistono nel database");
                    }

                    await GestisciPiatti(dishIds, menuId, conn, transaction);
                }

                await transaction.CommitAsync();
                return menuId;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task GestisciPiatti(List<int> dishIds, int menuId, SqlConnection conn, SqlTransaction transaction)
        {
            if (dishIds == null)
                return;

            // Elimino le relazioni esistenti
            string clearQuery = "DELETE FROM MenuDishes WHERE MenuId = @menuId";
            using (SqlCommand clearCmd = new(clearQuery, conn, transaction))
            {
                clearCmd.Parameters.AddWithValue("@menuId", menuId);
                await clearCmd.ExecuteNonQueryAsync();
            }

            // Inserisco le nuove relazioni
            string insertQuery = "INSERT INTO MenuDishes (MenuId, DishId) VALUES (@menuId, @dishId)";
            foreach (int dishId in dishIds)
            {
                using SqlCommand insertCmd = new(insertQuery, conn, transaction);
                insertCmd.Parameters.AddWithValue("@menuId", menuId);
                insertCmd.Parameters.AddWithValue("@dishId", dishId);
                await insertCmd.ExecuteNonQueryAsync();
            }
        }


        public async Task<int> UpdateMenu(int id, Menu menu)
        {
            using SqlConnection conn = new(connection_string);
            await conn.OpenAsync();

            using SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();

            try
            {
                // Prima verifico che il menu esista
                string checkMenuQuery = "SELECT COUNT(1) FROM Menus WHERE Id = @id";
                using (SqlCommand checkCmd = new(checkMenuQuery, conn, transaction))
                {
                    checkCmd.Parameters.AddWithValue("@id", id);
                    int exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                    if (exists == 0)
                    {
                        throw new InvalidOperationException($"Il menu con ID {id} non esiste");
                    }
                }

                // Aggiorno il nome del menu
                string updateQuery = "UPDATE Menus SET Name = @name WHERE Id = @id";
                using (SqlCommand cmd = new(updateQuery, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@name", menu.Name);
                    int affectedRows = await cmd.ExecuteNonQueryAsync();
                }

                // Gestisco i piatti se presenti
                if (menu.Dishes != null)
                {
                    var dishIds = menu.Dishes.Select(d => d.Id).ToList();

                    if (dishIds.Any())
                    {
                        // Verifico che tutti i piatti esistano
                        string verifyDishesQuery = @"
                    SELECT COUNT(*) 
                    FROM Dishes 
                    WHERE Id IN (" + string.Join(",", dishIds) + ")";

                        using SqlCommand verifyCmd = new(verifyDishesQuery, conn, transaction);
                        int existingDishCount = Convert.ToInt32(await verifyCmd.ExecuteScalarAsync());

                        if (existingDishCount != dishIds.Count)
                        {
                            throw new InvalidOperationException("Uno o più piatti specificati non esistono nel database");
                        }

                        // Uso il metodo GestisciPiatti con la transazione
                        await GestisciPiatti(dishIds, id, conn, transaction);
                    }
                    else
                    {
                        // Se la lista è vuota, rimuovo tutte le relazioni
                        string clearQuery = "DELETE FROM MenuDishes WHERE MenuId = @menuId";
                        using SqlCommand clearCmd = new(clearQuery, conn, transaction);
                        clearCmd.Parameters.AddWithValue("@menuId", id);
                        await clearCmd.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();
                return 1; // Ritorna 1 per indicare successo
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

 


        public async Task<int> DeleteMenu(int id)
        {
            using SqlConnection conn = new(connection_string);
            await conn.OpenAsync();

            using SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();

            try
            {
                // Prima elimino le relazioni nella tabella MenuDishes
                string deleteMenuDishesQuery = "DELETE FROM MenuDishes WHERE MenuId = @id";
                using (SqlCommand cmdMenuDishes = new(deleteMenuDishesQuery, conn, transaction))
                {
                    cmdMenuDishes.Parameters.AddWithValue("@id", id);
                    await cmdMenuDishes.ExecuteNonQueryAsync();
                }

                // Poi elimino il menu
                string deleteMenuQuery = "DELETE FROM Menus WHERE Id = @id";
                using SqlCommand cmdMenu = new(deleteMenuQuery, conn, transaction);
                cmdMenu.Parameters.AddWithValue("@id", id);
                int result = await cmdMenu.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                return result;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


      
    }
}
