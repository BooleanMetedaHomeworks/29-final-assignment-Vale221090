using System.Windows;
using System.Windows.Controls;
using ristorante_backend.Models;
using Menu = ristorante_backend.Models.Menu;

namespace ristorante_frontend.Views
{
    public partial class MenuView : Page
    {
        public MenuView()
        {
            InitializeComponent();
        }

        private void OnNewMenuClick(object sender, RoutedEventArgs e)
        {
            var newMenu = new Menu { Name = "Nuovo Menù" };
            MenuList.Items.Add(newMenu);
        }

        private void OnAddDishToMenu(object sender, RoutedEventArgs e)
        {
            if (MenuList.SelectedItem is Menu selectedMenu && DishList.SelectedItem is Dish selectedDish)
            {
                selectedMenu.Dishes.Add(selectedDish);
                // Opzionale: aggiornaro la vista
                DishList.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Seleziona un menù e un piatto prima di procedere.", "Attenzione",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void OnRemoveDishFromMenu(object sender, RoutedEventArgs e)
        {
            if (MenuList.SelectedItem is Menu selectedMenu && DishList.SelectedItem is Dish selectedDish)
            {
                selectedMenu.Dishes.Remove(selectedDish);
                
                DishList.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Seleziona un menù e un piatto da rimuovere prima di procedere.", "Attenzione",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}
