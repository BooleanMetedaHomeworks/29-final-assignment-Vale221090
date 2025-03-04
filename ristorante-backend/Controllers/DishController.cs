using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ristorante_backend.Models;
using ristorante_backend.Repositories;

namespace ristorante_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DishesController : ControllerBase
    {
        private readonly DishRepository _dishRepository;
        internal string connection_string = "Data Source=localhost;Initial Catalog=Ristorante;Integrated Security=True;Trust Server Certificate=True";

        public DishesController(DishRepository dishRepository)
        {
            _dishRepository = dishRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int? limit, int? categoryId)
        {
            try
            {
                if (categoryId.HasValue)
                {
                    var dishes = await _dishRepository.GetDishesByCategory(categoryId.Value);
                    return Ok(dishes);
                }

                var allDishes = await _dishRepository.GetDishes(limit);
                return Ok(allDishes);
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante il recupero dei piatti: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDishById(int id)
        {
            try
            {
                var dish = await _dishRepository.GetDishById(id);
                return dish == null ? NotFound($"Piatto con ID {id} non trovato") : Ok(dish);
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante il recupero del piatto: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Dish dish)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Errors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                    });
                }

                if (dish.Price <= 0)
                {
                    ModelState.AddModelError("Price", "Il prezzo deve essere maggiore di zero");
                    return BadRequest(ModelState);
                }

                // Verifica che la categoria esista prima di creare il piatto
                using (var connection = new SqlConnection(connection_string))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand(
                        "SELECT COUNT(1) FROM Categories WHERE Id = @CategoryId",
                        connection);
                    command.Parameters.AddWithValue("@CategoryId", dish.CategoryId);

                    var categoryExists = ((int?)await command.ExecuteScalarAsync()).GetValueOrDefault() > 0;

                    if (!categoryExists)
                    {
                        return BadRequest($"La categoria con ID {dish.CategoryId} non esiste");
                    }
                }

                dish.Id = 0; // Assicura che sia un nuovo piatto
                int dishId = await _dishRepository.CreateDishAsync(dish);

                // Recupera il piatto appena creato per la risposta
                var createdDish = await _dishRepository.GetDishById(dishId);
                return CreatedAtAction(nameof(GetDishById), new { id = dishId }, createdDish);
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante la creazione del piatto: {ex.Message}");
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Dish dish)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (dish.Price <= 0)
                {
                    return BadRequest("Il prezzo deve essere maggiore di zero");
                }

                var affectedRows = await _dishRepository.UpdateDish(id, dish);
                if (affectedRows == 0)
                {
                    return NotFound($"Piatto con ID {id} non trovato");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante l'aggiornamento del piatto: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var affectedRows = await _dishRepository.DeleteDish(id);
                if (affectedRows == 0)
                {
                    return NotFound($"Piatto con ID {id} non trovato");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante l'eliminazione del piatto: {ex.Message}");
            }
        }

        [HttpPut("{id}/price")]
        public async Task<bool> UpdatePriceAsync(int id, decimal newPrice)
        {
            using (var connection = new SqlConnection(connection_string))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    "UPDATE Dishes SET Price = @Price WHERE Id = @Id",
                    connection);

                command.Parameters.AddWithValue("@Price", newPrice);
                command.Parameters.AddWithValue("@Id", id);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }

        [HttpPut("{id}/category")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] int newCategoryId)
        {
            try
            {
                var dish = await _dishRepository.GetDishById(id);
                if (dish == null)
                {
                    return NotFound($"Piatto con ID {id} non trovato");
                }

                dish.CategoryId = newCategoryId;
                var affectedRows = await _dishRepository.UpdateDish(id, dish);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante l'aggiornamento della categoria: {ex.Message}");
            }
        }

    }
}
