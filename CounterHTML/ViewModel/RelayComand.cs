using System;
using System.Windows.Input;


namespace CounterHTML
{
    /// <summary>
    /// Класс реализующий RelayCommand, для привязки их в формах вью
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        /// <summary>Событие извещающее об измении состояния команды</summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>Конструктор команды</summary>
        /// <param name="execute">Выполняемый метод команды</param>
        /// <param name="canExecute">Метод разрешающий выполнение команды</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>Вызов разрешающего метода команды</summary>
        /// <param name="parameter">Параметр команды</param>
        /// <returns>True - если выполнение команды разрешено</returns>
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        /// <summary>Вызов выполняющего метода команды</summary>
        /// <param name="parameter">Параметр команды</param>
        public void Execute(object parameter) => _execute(parameter);
    }

}
