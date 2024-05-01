using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CounterHTML
{
    /// <summary>
    /// Базовый класс вью-модели, реализующий INotifyPropertyChanged
    /// </summary>
    public class BaseVM : INotifyPropertyChanged
    {
        // интерфейс обработчиков событий при изменении свойств
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
