using Microsoft.AspNetCore.Mvc;
using ristorante_backend.Models;
using ristorante_backend.Repositories;

namespace ristorante_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryRepository _categoryRepository;

        public CategoriesController(CategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string? name)
        {
            try
            {
                if (name == null)
                {
                    var categories = await _categoryRepository.GetCategories();
                    return Ok(categories);
                }
                else
                {
                    var categories = await _categoryRepository.GetCategories();
                    var filtered = categories.Where(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
                    return Ok(filtered);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante il recupero delle categorie: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryRepository.GetCategoryById(id);
                return category == null ? NotFound($"Categoria con ID {id} non trovata") : Ok(category);
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante il recupero della categoria: {ex.Message}");
            }
        }

        [HttpGet("withDishes")]
        public async Task<IActionResult> GetCategoriesWithDishes()
        {
            try
            {
                var categories = await _categoryRepository.GetCategoriesWithDishes();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante il recupero delle categorie con piatti: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                category.Id = 0; 
                int categoryId = await _categoryRepository.CreateCategoryAsync(category);

                // Recupero la categoria appena creata per la risposta
                var createdCategory = await _categoryRepository.GetCategoryById(categoryId);
                return CreatedAtAction(nameof(GetCategoryById), new { id = categoryId }, createdCategory);
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante la creazione della categoria: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category category)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var affectedRows = await _categoryRepository.UpdateCategory(id, category);
                if (affectedRows == 0)
                {
                    return NotFound($"Categoria con ID {id} non trovata");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante l'aggiornamento della categoria: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _categoryRepository.DeleteCategory(id);
                if (!success)
                {
                    return NotFound($"Categoria con ID {id} non trovata");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Errore durante l'eliminazione della categoria: {ex.Message}");
            }
        }

    }
}
