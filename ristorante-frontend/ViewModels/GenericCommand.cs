using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ristorante_frontend.ViewModels
{
    public class GenericCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<bool> _canExecute;

        public GenericCommand(Action<T> execute)
        {
            _execute = execute;
            _canExecute = () => true;
        }

        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) 
        {
            return _canExecute();
        }

        public void Execute(object parameter)
        {

            _execute((T)Convert.ChangeType(parameter, typeof(T)));
        }
    }
}