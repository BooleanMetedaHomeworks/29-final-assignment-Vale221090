using System.Collections.Generic;

namespace ristorante_backend.Models
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<Dish> Dishes { get; set; } = new List<Dish>();

        public Category() { }

        public Category(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
