using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ristorante_backend.Models;
using ristorante_backend.Repositories;
using ristorante_backend.Services;
using System.Text.Json.Serialization;
using System.Text;
using ristorante_backend.Loggers;

namespace ristorante_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<DishRepository>();
            builder.Services.AddSingleton<CategoryRepository>();
            builder.Services.AddSingleton<MenuRepository>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("DefaultCORS", builder =>
                {
                    // Aperto a tutto!
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                              Encoding.ASCII.GetBytes(
                                builder.Configuration.GetSection("JwtSettings")
                                                     .Get<JwtSettings>().Key)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            builder.Services.AddScoped<IPasswordHasher<UserModel>, PasswordHasher<UserModel>>();
            builder.Services.AddScoped<JwtAuthenticationService>();
            builder.Services.AddScoped<IUserService, UserService>();

           
            var app = builder.Build();

         
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication(); // Serve a JWT
            app.UseAuthorization();
            app.UseCors("DefaultCORS");

            app.MapControllers();

            app.Run();
        }
    }
}
