using System.Windows;

namespace ristorante_frontend.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void OnMenuClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MenuView());
        }

        private void OnDishesClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DishesView());
        }

        private void OnCategoriesClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CategoryView());
        }
    }
}
