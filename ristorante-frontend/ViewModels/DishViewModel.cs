using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using ristorante_frontend.Models;
using ristorante_frontend.Services;
using ristorante_frontend.ViewModels;
using FrontendDish = ristorante_frontend.Models.Dish;
using BackendDish = ristorante_backend.Models.Dish;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;


namespace ristorante_frontend.ViewModels
{
    public partial class DishViewModel : INotifyPropertyChanged
    {
        private readonly CollectionViewSource _dishesViewSource;
        private ObservableCollection<FrontendDish> _dishes;
        private FrontendDish? _selectedDish;
        private Category? _selectedCategory;
        private string _searchText = string.Empty;
        private bool _isAdmin;
        public bool IsAdmin
        {
            get => _isAdmin;
            set
            {
                if (_isAdmin != value)
                {
                    _isAdmin = value;
                    OnPropertyChanged();
                    NotifyCommandsCanExecuteChanged();
                }
            }
        }
        public string SearchText
        {
            get; set;
        } = string.Empty;

        private Jwt? _token;

        public ICollectionView DishesView => _dishesViewSource.View;

        // Rimosso required e init per evitare ambiguità
        private IAsyncRelayCommand _addDishCommand;
        private IAsyncRelayCommand<FrontendDish> _saveDishCommand;
        private IAsyncRelayCommand<FrontendDish> _deleteDishCommand;
        private IAsyncRelayCommand _assignToCategoryCommand;
        private IAsyncRelayCommand _removeFromCategoryCommand;

        public IAsyncRelayCommand AddDishCommand => _addDishCommand;
        public IAsyncRelayCommand<FrontendDish> SaveDishCommand => _saveDishCommand;
        public IAsyncRelayCommand<FrontendDish> DeleteDishCommand => _deleteDishCommand;
        public IAsyncRelayCommand AssignToCategoryCommand => _assignToCategoryCommand;
        public IAsyncRelayCommand RemoveFromCategoryCommand => _removeFromCategoryCommand;

        // Proprietà con nomi univoci
        private ObservableCollection<FrontendDish> DishCollection
        {
            get => _dishes;
            set
            {
                if (_dishes != value)
                {
                    _dishes = value;
                    _dishesViewSource.Source = _dishes;
                    OnPropertyChanged();
                }
            }
        }

        private FrontendDish? CurrentDish
        {
            get => _selectedDish;
            set
            {
                if (_selectedDish != value)
                {
                    _selectedDish = value;
                    OnPropertyChanged();
                    NotifyCommandsCanExecuteChanged();
                }
            }
        }

        private Category? CurrentCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged();
                    NotifyCommandsCanExecuteChanged();
                }
            }
        }

        private string CurrentSearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    FilterDishes();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

    


        private void NotifyCommandsCanExecuteChanged()
        {
            _addDishCommand.NotifyCanExecuteChanged();
            _saveDishCommand.NotifyCanExecuteChanged();
            _deleteDishCommand.NotifyCanExecuteChanged();
            _assignToCategoryCommand.NotifyCanExecuteChanged();
            _removeFromCategoryCommand.NotifyCanExecuteChanged();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        private async Task InitializeAsync()
        {
            try
            {
                var tokenResult = await ApiService.GetJwtToken();
                if (!tokenResult.IsConnectionSuccess)
                {
                    MessageBox.Show($"Errore di autenticazione: {tokenResult.ErrorMessage}");
                    return;
                }

                _token = tokenResult.Data;
                IsAdmin = _token?.Roles?.Contains("Admin") ?? false;

               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante l'inizializzazione: {ex.Message}");
            }
        }

        private void FilterDishes()
        {
            if (_dishesViewSource?.View == null) return;

            _dishesViewSource.View.Filter = item =>
            {
                if (string.IsNullOrWhiteSpace(_searchText)) return true;
                if (item is not FrontendDish dish) return false;

                return dish.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                       dish.Description?.Contains(_searchText, StringComparison.OrdinalIgnoreCase) == true;
            };
        }
    }
}

        