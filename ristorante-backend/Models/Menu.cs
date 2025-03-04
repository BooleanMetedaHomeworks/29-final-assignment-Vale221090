namespace ristorante_backend.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public ICollection<Dish> Dishes { get; set; } = new List<Dish>();

        public Menu() { }
        public Menu(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}
