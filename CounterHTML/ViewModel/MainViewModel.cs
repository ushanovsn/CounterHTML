using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;

namespace CounterHTML
{
    /// <summary>
    /// Тип делегата для вывода лога
    /// </summary>
    /// <param name="mess">Текст сообщения в лог</param>
    public delegate void dLogger(string mess);

    class MainViewModel : BaseVM
    {
        /// <summary>
        /// Экземпляр делегата для ведения лога
        /// </summary>
        dLogger makeLog;

        private string _urlAddress;
        /// <summary>
        /// Строка с адресом, по которому требуется выполнить анализ
        /// </summary>
        public string urlAddress
        {
            get => _urlAddress;
            set
            {
                _urlAddress = value;
                OnPropertyChanged();
            }
        }


        private bool _checkRegister;
        /// <summary>
        /// Флаг сравнения с учетом регистра
        /// </summary>
        public bool checkRegister
        {
            get => _checkRegister;
            set
            {
                _checkRegister = value;
                OnPropertyChanged();
            }
        }

        private string _logBox;
        /// <summary>
        /// Строка отображаемого лога
        /// </summary>
        public string logBox
        {
            get => _logBox;
            set
            {
                _logBox = value;
                OnPropertyChanged();
            }
        }

        private bool _blockUI;
        /// <summary>
        /// Флаг к которому привязываем блокировку интерфейса
        /// </summary>
        public bool blockUI
        {
            get => !_blockUI;
            set
            {
                _blockUI = value;
                OnPropertyChanged();
            }
        }

        private bool _needDumpFile;
        /// <summary>
        /// Флаг необходимости сохранять дамп страницы в файл
        /// </summary>
        public bool needDumpFile
        {
            get => _needDumpFile;
            set
            {
                _needDumpFile = value;
                OnPropertyChanged();
            }
        }

        private bool _needDumpLog = true;
        /// <summary>
        /// Флаг необходимости сохранять лог в файл
        /// </summary>
        public bool needDumpLog
        {
            get => _needDumpLog;
            set
            {
                _needDumpLog = value;
                OnPropertyChanged();
            }
        }

        private string _status;
        /// <summary>
        /// Строка статуса текущего процесса
        /// </summary>
        public string status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        private bool _acceptNumbers = true;
        /// <summary>
        /// Флаг учета чисел при анализе слов
        /// </summary>
        public bool acceptNumbers
        {
            get => _acceptNumbers;
            set
            {
                _acceptNumbers = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Список строк для отображения (результат)
        /// </summary>
        public ObservableCollection<StatisticRow >resultList { get; set; }

        





        private ICommand _makeAnalize;
        /// <summary>
        /// Команда для кнопки ОК, начала выполнения анализа страницы сайта
        /// </summary>
        public ICommand makeAnalize => _makeAnalize ?? (_makeAnalize = new RelayCommand(parameter =>
            {
                //TODO Адекватная проверка и корректировка адреса
                if (!isValidAddr(urlAddress))
                    return;
                
                logThisMess("Выполняем анализ слов со страницы по адресу: " + urlAddress);
                makeAnalizeAsync();
            }
            ));

        
        /// <summary>
        /// Конструктор класса 
        /// </summary>
        public MainViewModel()
        {
            resultList = new ObservableCollection<StatisticRow>();
            makeLog = new dLogger(logThisMess);
            status = "Готов";

            logThisMess("Стартовые операции выполнены.");
        }

        /// <summary>
        /// Корректное завершение работы и удаление объектов класса
        /// </summary>
        public void Dispose()
        {
            Logger.Flush();
        }


        /// <summary>
        /// Вывод лога (также используется в делегате)
        /// </summary>
        /// <param name="mess">Сообщение для вывода</param>
        private void logThisMess (string mess)
        {
            logBox += DateTime.Now.ToString() + ": " + mess + "\n";
            if (needDumpLog)
            {
                Logger.WriteLog(mess);
            }
        }


        /// <summary>
        /// Асинхронный метод проверки содержимого указанного адреса
        /// </summary>
        private async void makeAnalizeAsync()
        {
            blockUI = true;
            status = "Выполняем анализ";
            var newResultCollection = new List<StatisticRow>();
            await Task.Run(() =>
            {
                MainModel model = new MainModel(makeLog);
                newResultCollection = model.AnalizeUrl(urlAddress, needDumpFile, checkRegister, acceptNumbers);
            });
            status = "Анализ выполнен. Обрабатываем результат";
            urlAddress = "";
            resultList.Clear();
            if (newResultCollection == null)
            {
                status = "В процессе анализа произошла ошибка, попробуйте еще раз";
            }
            else if(newResultCollection?.Count == 0)
            {
                status = "По указанному адресу не обнаружено текстовой информации";
            }
            else
            {
                foreach (var item in newResultCollection)
                {
                    resultList.Add(item);
                }
                logThisMess("Анализ страницы завершен успешно, найдено " + resultList.Count + " уникальных слов");
                status = "Найдено слов: " + resultList.Count;
            }

            blockUI = false;
        }

        /// <summary>
        /// Проверка на правильный формат адреса
        /// </summary>
        /// <param name="url">Адрес для проверки</param>
        /// <returns>true - если адрес корректен, false - если адрес некорректный</returns>
        private bool isValidAddr(string url)
        {
            bool result = true;

            if (url == null || url == "")
            {
                logThisMess("Адрес не может быть пустым.");
                result = false;
                return result;
            }

            if (!(url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            {
                logThisMess("Адрес должен начинаться с http://");
                result = false;
            }

            return result;
        }




    }
}
