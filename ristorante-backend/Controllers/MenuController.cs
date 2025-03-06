using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ristorante_backend.Models;
using ristorante_backend.Repositories;

namespace ristorante_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly MenuRepository _menuRepository;
        private string connection_string= "Data Source=localhost;Initial Catalog=Ristorante;Integrated Security=True;Pooling=False;Encrypt=True;Trust Server Certificate=True";

        public MenuController(MenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string? name, int? limit)
        {
            try
            {
                var menus = await _menuRepository.GetMenus(limit);
                if (name != null)
                {
                    menus = menus.Where(m => m.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                return Ok(menus);
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante il recupero dei menù: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMenuById(int id)
        {
            try
            {
                var menu = await _menuRepository.GetMenuById(id);
                return menu == null ? NotFound($"Menù con ID {id} non trovato") : Ok(menu);
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante il recupero del menù: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Menu menu)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                menu.Id = 0; // Mi assicuro che sia un nuovo menù
                int menuId = await _menuRepository.CreateMenuAsync(menu);

                // Recupero il menù appena creato per la risposta
                var createdMenu = await _menuRepository.GetMenuById(menuId);
                return CreatedAtAction(nameof(GetMenuById), new { id = menuId }, createdMenu);
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante la creazione del menù: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Menu menu)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var affectedRows = await _menuRepository.UpdateMenu(id, menu);
                if (affectedRows == 0)
                {
                    return NotFound($"Menù con ID {id} non trovato");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante l'aggiornamento del menù: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var affectedRows = await _menuRepository.DeleteMenu(id);
                if (affectedRows == 0)
                {
                    return NotFound($"Menù con ID {id} non trovato");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante l'eliminazione del menù: {ex.Message}");
            }
        }

     
        [HttpPost("{menuId}/dishes/{dishId}")]
        public async Task<IActionResult> AddDishToMenu(int menuId, int dishId)
        {
            try
            {
                using var conn = new SqlConnection(connection_string);
                await conn.OpenAsync();

                // Prima verifico se la relazione già esiste
                string checkQuery = "SELECT COUNT(1) FROM MenuDishes WHERE MenuId = @MenuId AND DishId = @DishId";
                using (var checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@MenuId", menuId);
                    checkCmd.Parameters.AddWithValue("@DishId", dishId);
                    int exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                    if (exists > 0)
                    {
                        return BadRequest("Il piatto è già presente nel menu");
                    }
                }

                // Verifico se il menu esiste
                string checkMenuQuery = "SELECT COUNT(1) FROM Menus WHERE Id = @MenuId";
                using (var checkMenuCmd = new SqlCommand(checkMenuQuery, conn))
                {
                    checkMenuCmd.Parameters.AddWithValue("@MenuId", menuId);
                    int menuExists = Convert.ToInt32(await checkMenuCmd.ExecuteScalarAsync());

                    if (menuExists == 0)
                    {
                        return NotFound($"Menu con ID {menuId} non trovato");
                    }
                }

                // Verifico se il piatto esiste
                string checkDishQuery = "SELECT COUNT(1) FROM Dishes WHERE Id = @DishId";
                using (var checkDishCmd = new SqlCommand(checkDishQuery, conn))
                {
                    checkDishCmd.Parameters.AddWithValue("@DishId", dishId);
                    int dishExists = Convert.ToInt32(await checkDishCmd.ExecuteScalarAsync());

                    if (dishExists == 0)
                    {
                        return NotFound($"Piatto con ID {dishId} non trovato");
                    }
                }

                // Se arriviamo qui, posso procedere con l'inserimento
                var query = "INSERT INTO MenuDishes (MenuId, DishId) VALUES (@MenuId, @DishId)";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MenuId", menuId);
                cmd.Parameters.AddWithValue("@DishId", dishId);

                await cmd.ExecuteNonQueryAsync();
                return Ok("Piatto aggiunto al menu con successo");
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante l'aggiunta del piatto al menu: {ex.Message}");
            }
        }



        [HttpDelete("{menuId}/dishes/{dishId}")]
        public async Task<int> RemoveDishFromMenu(int menuId, int dishId)
        {
            using var conn = new SqlConnection(connection_string);
            await conn.OpenAsync();

            const string query = "DELETE FROM MenuDishes WHERE MenuId = @MenuId AND DishId = @DishId";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@MenuId", menuId);
            cmd.Parameters.AddWithValue("@DishId", dishId);

            return await cmd.ExecuteNonQueryAsync();
        }

    }
}
