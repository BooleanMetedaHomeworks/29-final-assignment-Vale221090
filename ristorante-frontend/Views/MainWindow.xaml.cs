using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ristorante_frontend.ViewModels;

namespace ristorante_frontend.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.NavigationMenu.Visibility = Visibility.Collapsed;
            }
        }
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Mostra/Nascondi il menu in base alla pagina corrente
            NavigationMenu.Visibility = e.Content is LoginPage
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
        private void NavigateToCategories(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CategoryView());
        }

        private void NavigateToDishes(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ListView());
        }

        private void NavigateToMenus(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MenuView());
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
