using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;

namespace CounterHTML
{
    /// <summary>
    /// Логер. Файловый поток открывается один раз и далее просто используется.
    /// По окончанию вызывать Flush, для корректного завершения и освобождения ресурсов.
    /// </summary>
    internal static class Logger
    {
        /// <summary>
        /// Потокобезопасная коллекция, с реализацией очереди
        /// </summary>
        private static BlockingCollection<string> _blockingCollection;
        /// <summary>
        /// Наименование файла для ведения логов
        /// </summary>
        private static string _filename = "CounterHTML.log";
        /// <summary>
        /// Задача для запуска в ней процесса логирования (записи в файл)
        /// </summary>
        private static Task _task;

        /// <summary>
        /// Отдельный таск зациклен на отслеживание очереди и записи данных из очереди в файл
        /// </summary>
        static Logger()
        {
            // создаем экземпляр потокобезпасной коллекции
            _blockingCollection = new BlockingCollection<string>();

            // запуск задачи с процессом логирования
            _task = Task.Factory.StartNew(() =>
            {
                using (var streamWriter = new StreamWriter(_filename, true, Encoding.UTF8))
                {
                    // корректное авто-завершение при краше, чтобы не потерять данные
                    streamWriter.AutoFlush = true;
                    // на методе GetConsumingEnumerable задача будет постоянно в ожидании новых данных,
                    // как только появляются новые данные - он будет выполняться.
                    // выход из цикла произойдёт только после вызова метода _blockingCollection.CompleteAdding
                    foreach (var s in _blockingCollection.GetConsumingEnumerable())
                        streamWriter.WriteLine(s);
                }
            },
            TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Метод передачи данных на запись в лог,
        /// отрабатывает максимально быстро (просто добавляя сообщение в очередь потокобезопасной коллекции)
        /// </summary>
        /// <param name="message"> Структура с сообщением, временем и именем сгенерировавшего сообщение модуля</param>
        public static void WriteLog(string message)
        {
            _blockingCollection.Add(DateTime.Now.ToString() + ": " + message + "\n");
        }

        /// <summary>
        /// Метод корректного завершения логера.
        /// </summary>
        public static void Flush()
        {
            // метод завершает цикл foreach (var s in _blockingCollection.GetConsumingEnumerable()) в основном таске
            _blockingCollection.CompleteAdding();
            // ожидание завершения
            _task.Wait();
        }
    }
}
