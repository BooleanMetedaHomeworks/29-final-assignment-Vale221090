using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ristorante_frontend.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ristorante_frontend.ViewModels
{
    public partial class MenuViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Menu> menus = new();

        [ObservableProperty]
        private ObservableCollection<Dish> availableDishes = new();

        [ObservableProperty]
        private Menu? selectedMenu;

        [ObservableProperty]
        private Dish? selectedDish;

        [RelayCommand]
        private void AddNewMenu()
        {
            var newMenu = new Menu { Name = "Nuovo Menù" };
            Menus.Add(newMenu);
            SelectedMenu = newMenu;
        }

        [RelayCommand]
        private void AddDishToMenu()
        {
            if (SelectedMenu != null && SelectedDish != null)
            {
                SelectedMenu.Dishes.Add(SelectedDish);
                OnPropertyChanged(nameof(SelectedMenu));
            }
        }

        [RelayCommand]
        private void RemoveDishFromMenu()
        {
            if (SelectedMenu != null && SelectedDish != null)
            {
                SelectedMenu.Dishes.Remove(SelectedDish);
                OnPropertyChanged(nameof(SelectedMenu));
            }
        }
    }
}
