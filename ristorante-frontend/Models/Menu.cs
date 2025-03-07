using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ristorante_frontend.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public ICollection<Dish> Dishes { get; set; } = new List<Dish>();

        public override string ToString() => Name;
    }
}
