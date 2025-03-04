using Nest;

namespace ristorante_backend.Models
{
    public class Dish
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public ICollection<Menu> Menus { get; set; } = new List<Menu>();


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