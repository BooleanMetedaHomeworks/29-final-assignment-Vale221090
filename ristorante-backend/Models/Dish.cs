using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace ristorante_backend.Models
{
   

    public class Dish
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Il nome è obbligatorio")]
        public required string Name { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Il prezzo è obbligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Il prezzo deve essere maggiore di zero")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "La categoria è obbligatoria")]
        public int CategoryId { get; set; }

        [JsonIgnore]
        public Category? Category { get; set; }

        [JsonIgnore]
        public ICollection<Menu>? Menus { get; set; }


        public Dish()
        {

        }

        public Dish(int id, string name, string description, decimal price, int categoryId)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            CategoryId = categoryId;
        }
    }
}