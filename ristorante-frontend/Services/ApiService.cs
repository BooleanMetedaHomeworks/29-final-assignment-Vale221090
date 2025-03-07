using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ristorante_frontend.Models;
using System.Net.Http.Headers;
using System.Security.Claims;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;



namespace ristorante_frontend.Services

{
    public enum ApiServiceResultType
    {
        Success,
        Error
    }
    public static class ApiService
    {
        private const string API_URL = "http://localhost:5194";
        public static string Email { get; set; }
        public static string Password { get; set; }
        public static async Task<ApiServiceResult<bool>> Register()
        {
            try
            {
                using HttpClient client = new HttpClient();
                var httpResult = await client.PostAsJsonAsync($"{API_URL}/Account/Register",
                    JsonContent.Create(new { Email, Password }));
                var resultBody = await httpResult.Content.ReadAsStringAsync();
                var data = httpResult.IsSuccessStatusCode;
                return new ApiServiceResult<bool>(data);
            }
            catch (Exception e)
            {
                return new ApiServiceResult<bool>(e);
            }
        }

        public static async Task<ApiServiceResult<Jwt>> GetJwtToken()
        {
            try
            {
                using HttpClient client = new HttpClient();
                var httpResult = await client.PostAsJsonAsync($"{API_URL}/Account/Login",
                    JsonContent.Create(new { Email = Email, Password = Password }));
                var resultBody = await httpResult.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Jwt>(resultBody);
                if (data.Token == null)
                {
                    return new ApiServiceResult<Jwt>(new Exception("Login fallito"));
                }
                AddRolesToJwt(data);
                return new ApiServiceResult<Jwt>(data);
            }
            catch (Exception e)
            {
                return new ApiServiceResult<Jwt>(e);
            }
        }
        private static void AddRolesToJwt(Jwt jwt)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt.Token);
                var roles = token.Claims.Where((Claim c) => c.Type == "role").Select(c => c.Value).ToList();
                jwt.Roles = roles;
            }
            catch { }

        }
        public static async Task<ApiServiceResult<List<Menu>>> Get()
        {
            try
            {
                using HttpClient client = new HttpClient();
                var httpResult = await client.GetAsync($"{API_URL}/Menu");
                var resultBody = await httpResult.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<Menu>>(resultBody);
                return new ApiServiceResult<List<Menu>>(data);
            }
            catch (Exception e)
            {
                return new ApiServiceResult<List<Menu>>(e);
            }
        }
        public static async Task<ApiServiceResult<int>> Create(Menu newMenu, Jwt jwt)
        {
            try
            {
                using HttpClient httpclient = new HttpClient();
                httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt.Token);
                var httpResult = await httpclient.PostAsync($"{API_URL}/Menu", JsonContent.Create(newMenu));
                var resultBody = await httpResult.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<int>(resultBody);

                return new ApiServiceResult<int>(data);
            }
            catch (Exception e)
            {
                return new ApiServiceResult<int>(e);
            }
        }

        public static async Task<ApiServiceResult<int>> Update(Menu menu, Jwt token)
        {
            try
            {
                using HttpClient httpclient = new ();
                httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

                var httpResult = await httpclient.PutAsync($"{API_URL}/Menu/{menu.Id}", JsonContent.Create(menu));
                var resultBody = await httpResult.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<int>(resultBody);
                return new ApiServiceResult<int>(data);
            }
            catch (Exception e)
            {
                return new ApiServiceResult<int>(e);
            }
        }
        
        
        public static async Task<ApiServiceResult<int>> Delete(int id, Jwt token)
        {
            try
            {
                using HttpClient httpclient = new HttpClient();
                httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                var httpResult = await httpclient.DeleteAsync($"{API_URL}/Menu/{id}");
                var resultBody = await httpResult.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<int>(resultBody);
                return new ApiServiceResult<int>(data);
            }
            catch (Exception e)
            {
                return new ApiServiceResult<int>(e);
            }

        }

        internal static async Task Delete(Dish dish, Jwt token)
        {
            throw new NotImplementedException();
        }

        internal static async Task Create(Dish newDish, Jwt jwt)
        {
            throw new NotImplementedException();
        }

        internal static async Task Create(Category newCategory, Jwt? token)
        {
            throw new NotImplementedException();
        }

        internal static async Task Update(Category category, Jwt? token)
        {
            throw new NotImplementedException();
        }

        
        internal static async Task CreateDish(Dish newDish, string v)
        {
            throw new NotImplementedException();
        }

        internal static async Task GetDishes()
        {
            throw new NotImplementedException();
        }
    } 
}