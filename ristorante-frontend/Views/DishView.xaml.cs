using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ristorante_frontend.Models;

namespace ristorante_frontend.Views
{
    public partial class DishesView : Page
    {
        // CollectionViewSource per filtrare i piatti
        private CollectionViewSource dishesViewSource;

        public DishesView()
        {
            InitializeComponent();
            InitializeCollectionView();
        }

        private void InitializeCollectionView()
        {
            dishesViewSource = new CollectionViewSource();
            DishesList.ItemsSource = dishesViewSource.View;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (dishesViewSource != null && textBox != null)
            {
                dishesViewSource.View.Filter = item =>
                {
                    if (string.IsNullOrEmpty(textBox.Text)) return true;
                    var dish = item as Dish;
                    return dish?.Name.Contains(textBox.Text, StringComparison.OrdinalIgnoreCase) ?? false;
                };
            }
        }

        private void OnAddDishClick(object sender, RoutedEventArgs e)
        {
            
            MessageBox.Show("Aggiunta nuovo piatto");
        }

        private void OnEditDishClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dish = button?.DataContext as Dish;
            if (dish != null)
            {
                
                MessageBox.Show($"Modifica del piatto: {dish.Name}");
            }
        }

        private void OnDeleteDishClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dish = button?.DataContext as Dish;
            if (dish != null)
            {
                var result = MessageBox.Show(
                    $"Sei sicuro di voler eliminare il piatto {dish.Name}?",
                    "Conferma eliminazione",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Rimuovo il piatto dalla CollectionViewSource
                        if (dishesViewSource.Source is System.Collections.IList list)
                        {
                            list.Remove(dish);
                            dishesViewSource.View.Refresh();
                            MessageBox.Show($"Il piatto {dish.Name} è stato eliminato con successo.",
                                "Eliminazione completata",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore durante l'eliminazione del piatto: {ex.Message}",
                            "Errore",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }


        private void OnMoveToCategory(object sender, RoutedEventArgs e)
        {
            var selectedDish = DishesList.SelectedItem as Dish;
            var selectedCategory = CategoriesList.SelectedItem as Category;

            if (selectedDish != null && selectedCategory != null)
            {
                try
                {
                    // Salvo la vecchia categoria per il messaggio
                    var oldCategory = selectedDish.Category?.Name ?? "nessuna categoria";

                    // Aggiorno la categoria del piatto
                    selectedDish.Category = selectedCategory;

                    // Aggiorno la vista
                    if (dishesViewSource != null)
                    {
                        dishesViewSource.View.Refresh();
                    }

                    MessageBox.Show(
                        $"Il piatto '{selectedDish.Name}' è stato spostato dalla categoria '{oldCategory}' alla categoria '{selectedCategory.Name}'",
                        "Spostamento completato",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Errore durante lo spostamento del piatto: {ex.Message}",
                        "Errore",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(
                    "Seleziona un piatto e una categoria",
                    "Attenzione",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }


        private void OnRemoveFromCategory(object sender, RoutedEventArgs e)
        {
            var selectedDish = DishesList.SelectedItem as Dish;
            if (selectedDish != null)
            {
                if (selectedDish.Category != null)
                {
                    try
                    {
                        // Salvo il nome della categoria per il messaggio
                        var oldCategory = selectedDish.Category.Name;

                        // Rimuovo la categoria dal piatto
                        selectedDish.Category = null;

                        // Aggiorno la vista
                        if (dishesViewSource != null)
                        {
                            dishesViewSource.View.Refresh();
                        }

                        MessageBox.Show(
                            $"Il piatto '{selectedDish.Name}' è stato rimosso dalla categoria '{oldCategory}'",
                            "Rimozione completata",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Errore durante la rimozione del piatto dalla categoria: {ex.Message}",
                            "Errore",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Il piatto selezionato non appartiene a nessuna categoria",
                        "Attenzione",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

            }
            else
            {
                MessageBox.Show("Seleziona un piatto da rimuovere dalla categoria",
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
