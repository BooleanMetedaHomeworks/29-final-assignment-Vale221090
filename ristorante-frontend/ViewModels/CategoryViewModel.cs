using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ristorante_frontend.ViewModels
{
    public partial class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<DishViewModel> Dishes { get; set; } = new();
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public int? CategoryId { get; set; }
  
        public ICollection<MenuViewModel> Menus { get; set; }

        public override string ToString() => Name;

    }
}
