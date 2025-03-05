using System.Windows;
using System.Windows.Controls;

namespace ristorante_frontend.Views
{
    public partial class CategoryView : Page
    {
        public CategoryView()
        {
            InitializeComponent();
            LoadCategories();
        }

        private async void LoadCategories()
        {
            try
            {
                // TODO: Implementare la chiamata al service per caricare le categorie
                // CategoriesList.ItemsSource = await categoriesService.GetAllCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel caricamento delle categorie: {ex.Message}",
                              "Errore",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Implementare la logica di filtro
        }

        private void OnAddCategoryClick(object sender, RoutedEventArgs e)
        {
            // TODO: Aprire dialog per nuova categoria
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            // TODO: Aprire dialog modifica categoria
        }

        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            // TODO: Implementare la logica di eliminazione con conferma
        }
    }
}
