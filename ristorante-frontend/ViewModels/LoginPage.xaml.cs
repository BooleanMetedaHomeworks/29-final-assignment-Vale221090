using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ristorante_frontend.ViewModels
{
    /// <summary>
    /// Logica di interazione per LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }
        private void OnLoginButtonClick(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("Views/DishView.xaml", UriKind.Relative));
        }

    }
}
