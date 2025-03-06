using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ristorante_frontend.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using FrontendDish = ristorante_frontend.Models.Dish;
using BackendDish = ristorante_backend.Models.Dish;

namespace ristorante_frontend.ViewModels
{
    public partial class DishViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly ICollectionView _dishesView;

        [ObservableProperty]
        private ObservableCollection<FrontendDish> dishes = new();

        [ObservableProperty]
        private FrontendDish? selectedDish;

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isEditingValue;

        public bool IsEditing { get; private set; }

        public DishViewModel(ApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _dishesView = CollectionViewSource.GetDefaultView(Dishes);
            _dishesView.Filter = DishFilter;

            Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await LoadDishesAsync();
            });
        }

        private BackendDish ConvertToBackendDish(FrontendDish frontendDish)
        {
            return new BackendDish
            {
                Id = frontendDish.Id,
                Name = frontendDish.Name,
                Description = frontendDish.Description,
                Price = frontendDish.Price,
                CategoryId = frontendDish.CategoryId
            };
        }

        private FrontendDish ConvertToFrontendDish(BackendDish backendDish)
        {
            return new FrontendDish
            {
                Id = backendDish.Id,
                Name = backendDish.Name,
                Description = backendDish.Description,
                Price = backendDish.Price,
                CategoryId = backendDish.CategoryId
            };
        }

        private async Task LoadDishesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var dishList = await _apiService.GetDishesAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Dishes.Clear();
                    foreach (var backendDish in dishList)
                    {
                        Dishes.Add(ConvertToFrontendDish(backendDish));
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Errore nel caricamento dei piatti: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanAddDish))]
        private async Task AddDishAsync()
        {
            try
            {
                IsEditing = true;
                var newDish = new FrontendDish
                {
                    Name = "Nuovo Piatto",
                    Description = "Inserire descrizione",
                    Price = 0.00m
                };

                var backendDish = ConvertToBackendDish(newDish);
                var createdBackendDish = await _apiService.CreateDishAsync(backendDish);

                if (createdBackendDish != null)
                {
                    var createdFrontendDish = ConvertToFrontendDish(createdBackendDish);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Dishes.Add(createdFrontendDish);
                        SelectedDish = createdFrontendDish;
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Errore durante l'aggiunta del piatto: {ex.Message}";
            }
            finally
            {
                IsEditing = false;
            }
        }

        private bool CanAddDish() => !IsLoading && !IsEditing;

        [RelayCommand(CanExecute = nameof(CanEditDish))]
        private async Task EditDishAsync(FrontendDish dish)
        {
            if (dish == null) return;

            try
            {
                IsEditing = true;
                var backendDish = ConvertToBackendDish(dish);
                await _apiService.UpdateDishAsync(dish.Id, backendDish);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var index = Dishes.IndexOf(dish);
                    if (index != -1)
                    {
                        Dishes[index] = dish;
                        _dishesView.Refresh();
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Errore durante la modifica del piatto: {ex.Message}";
            }
            finally
            {
                IsEditing = false;
            }
        }

        private bool CanEditDish() => SelectedDish != null && !IsLoading && !IsEditing;

        [RelayCommand(CanExecute = nameof(CanDeleteDish))]
        private async Task DeleteDishAsync(FrontendDish dish)
        {
            if (dish == null) return;

            var result = MessageBox.Show(
                $"Sei sicuro di voler eliminare il piatto {dish.Name}?",
                "Conferma eliminazione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                await _apiService.DeleteDishAsync(dish.Id);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Dishes.Remove(dish);
                    if (SelectedDish == dish)
                    {
                        SelectedDish = null;
                    }
                    _dishesView.Refresh();
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Errore durante l'eliminazione del piatto: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanDeleteDish() => SelectedDish != null && !IsLoading && !IsEditing;

        private bool DishFilter(object item)
        {
            if (string.IsNullOrEmpty(SearchText)) return true;
            if (item is not FrontendDish dish) return false;

            return dish.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                   (dish.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        partial void OnSearchTextChanged(string value)
        {
            Application.Current.Dispatcher.Invoke(() => _dishesView.Refresh());
        }

        [RelayCommand(CanExecute = nameof(CanRefresh))]
        private async Task RefreshAsync()
        {
            await LoadDishesAsync();
        }

        private bool CanRefresh() => !IsLoading && !IsEditing;
    }
}
