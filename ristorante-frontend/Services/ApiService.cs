using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using ristorante_backend.Models;

namespace ristorante_frontend.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:4217";
        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        // API Menu
        public async Task<List<Menu>> GetMenusAsync(string? name = null, int? limit = null)
        {
            var query = new List<string>();
            if (name != null) query.Add($"name={Uri.EscapeDataString(name)}");
            if (limit != null) query.Add($"limit={limit}");

            var url = "Menu" + (query.Any() ? "?" + string.Join("&", query) : "");
            return await _httpClient.GetFromJsonAsync<List<Menu>>(url) ?? new List<Menu>();
        }

        public async Task<Menu?> GetMenuByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Menu>($"Menu/{id}");
        }

        public async Task<Menu?> CreateMenuAsync(Menu menu)
        {
            var response = await _httpClient.PostAsJsonAsync("Menu", menu);
            return await response.Content.ReadFromJsonAsync<Menu>();
        }

        public async Task UpdateMenuAsync(int id, Menu menu)
        {
            await _httpClient.PutAsJsonAsync($"Menu/{id}", menu);
        }

        public async Task DeleteMenuAsync(int id)
        {
            await _httpClient.DeleteAsync($"Menu/{id}");
        }

        public async Task AddDishToMenuAsync(int menuId, int dishId)
        {
            await _httpClient.PostAsync($"Menu/{menuId}/dishes/{dishId}", null);
        }

        public async Task RemoveDishFromMenuAsync(int menuId, int dishId)
        {
            await _httpClient.DeleteAsync($"Menu/{menuId}/dishes/{dishId}");
        }

        // API Piatti
        public async Task<List<Dish>> GetDishesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Dish>>("Dish") ?? new List<Dish>();
        }

        public async Task<Dish?> GetDishByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Dish>($"Dish/{id}");
        }

        public async Task<Dish?> CreateDishAsync(Dish dish)
        {
            var response = await _httpClient.PostAsJsonAsync("Dish", dish);
            return await response.Content.ReadFromJsonAsync<Dish>();
        }

        public async Task UpdateDishAsync(int id, Dish dish)
        {
            await _httpClient.PutAsJsonAsync($"Dish/{id}", dish);
        }

        public async Task DeleteDishAsync(int id)
        {
            await _httpClient.DeleteAsync($"Dish/{id}");
        }

        // API Categorie
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Category>>("Category") ?? new List<Category>();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Category>($"Category/{id}");
        }

        public async Task<Category?> CreateCategoryAsync(Category category)
        {
            var response = await _httpClient.PostAsJsonAsync("Category", category);
            return await response.Content.ReadFromJsonAsync<Category>();
        }

        public async Task UpdateCategoryAsync(int id, Category category)
        {
            await _httpClient.PutAsJsonAsync($"Category/{id}", category);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            await _httpClient.DeleteAsync($"Category/{id}");
        }
    }
}
