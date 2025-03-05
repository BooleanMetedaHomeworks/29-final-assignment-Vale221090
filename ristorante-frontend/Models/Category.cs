using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ristorante_frontend.Models
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<Dish> Dishes { get; set; } = new();

        public override string ToString() => Name;
    }

}
