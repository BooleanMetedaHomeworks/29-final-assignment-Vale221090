using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ristorante_frontend.Models;
using ristorante_frontend.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ristorante_frontend.ViewModels
{
    public partial class MenuViewModel : INotifyPropertyChanged
    {

        private ObservableCollection<Menu> _menus;
        private ObservableCollection<Menu> Menus
        {
            get => _menus;
            set
            {
                if (value == _menus) { return; }
                _menus = value;
                OnPropertyChanged(nameof(Menus));
            }
        }

        public ICommand AddMenusCommand { get; private set; }
        public ICommand SaveMenuCommand { get; private set; }
        public ICommand DeleteMenuCommand { get; set; }

        public bool IsAdmin { get; set; }

        private Jwt _token;

        public MenuViewModel()
        {
            _ = Initialize();
            this.AddMenusCommand = new MyCommand(async () =>
            {
                Menu newMenu = new Menu()
                {
                    Id = 0,
                    Name = "Nuovo Menu",
                    Dishes = new ObservableCollection<Dish>()

                };
                var createApiResult = await ApiService.Create(newMenu, _token);
                if ((createApiResult.Data == null))
                {
                    MessageBox.Show($"Errore: {createApiResult.ErrorMessage}");
                    return;
                }
                newMenu.Id = createApiResult.Data;
                Menus.Add(newMenu);

            });
            this.SaveMenuCommand = new GenericCommand<Menu>(async (menu) =>
            {
                var updateApiResult = await ApiService.Update(menu, _token);
                if (updateApiResult.Data == 0)
                {
                    MessageBox.Show($"Errore: {updateApiResult.ErrorMessage}");
                }
            });
            this.DeleteMenuCommand = new GenericCommand<Menu>(async (menu) =>
            {
                var deleteApiResult = await ApiService.Delete(menu.Id, _token);
                if (deleteApiResult.Data == 0)
                {
                    MessageBox.Show($"Errore: {deleteApiResult.ErrorMessage}");
                    return;

                }
                Menus.Remove(menu);
            });
        }
        public async Task Initialize()
        {
            var tokenApiResult = await ApiService.GetJwtToken();
            if (tokenApiResult.Data == null)
            {
                MessageBox.Show($"Errore di login! {tokenApiResult.ErrorMessage}");
                return;
            }
            this._token = tokenApiResult.Data;
            this.IsAdmin = _token.Roles.Any(x=> x == "Admin");

            var menusApiResult = await ApiService.Get();
            if (menusApiResult.Data == null)
            {
                MessageBox.Show($"Errore: {menusApiResult.ErrorMessage}");
                return;
            }
            Menus = new ObservableCollection<Menu>(menusApiResult.Data);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
