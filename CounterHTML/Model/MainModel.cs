using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Net;
using System.IO;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;

namespace CounterHTML
{
    class MainModel
    {
        /// <summary>
        /// Используем делегат для вывода сообщений со всех методов класса
        /// </summary>
        dLogger logger;

        /// <summary>
        /// Массив разделителей слов для текста
        /// </summary>
        char[] splitters = new char[] { ' ', ',', '.', '!', '?', '"', ';', ':', '[', ']', '(', ')', '\n', '\r', '\t', '*', '/', '\\', '|', '+', '=', '–', '~', '«', '»' };


        /// <summary>
        /// Конструктор класса инициализирующий глобальные объекты класса
        /// </summary>
        /// <param name="log">Делегат - логер, для вывода сообщений из этого класса</param>
        public MainModel(dLogger log)
        {
            // указываем логер для всего класса
            logger = log;
        }

        /// <summary>
        /// Деструктор класса
        /// </summary>
        public void Dispose()
        {

        }

        /// <summary>
        /// Основная функция выполнения анализа текста сайта по адресу
        /// </summary>
        /// <param name="url">Адрес страницы, текст которой требуется проанализировать</param>
        /// <param name="needDump">Флаг сохранения обрабатываемых данных в файлы</param>
        /// <param name="needRegister">Флаг сравнения слов, с учетом регистра букв</param>
        /// <param name="acceptNumbers">Флаг учета чисел, в качестве отдельных слов</param>
        /// <returns>Список элементов для отображения в таблице. Если возвращается null - в процессе работы произошла ошибка</returns>
        public List<StatisticRow> AnalizeUrl (string url, bool needDump, bool needRegister, bool acceptNumbers)
        {
            string text = getContent(url, needDump);
            if (text == "")
            {
                logger("Не удалось получить данные по введенному адресу. Повторите после исправления.");
                return null;
            }

            //TODO выполнить анализ внутри метода и вернуть готовый список (для оптимизации использования памяти)
            // получаем список всех слов со страницы
            List<string> strings = GetAllTexts(text, needDump);

            if (strings?.Count == 0)
                return null;

            Dictionary<string, int> findedWords = parseStringsToWords(strings, needRegister);

            var resultList = new List<StatisticRow>();
            int i = 0;  //порядковый номер
            foreach (var item in findedWords)
            {
                if (acceptNumbers)
                {
                    resultList.Add(new StatisticRow(++i, item.Key, item.Value));
                }
                else
                {
                    if (!int.TryParse(item.Key, out int strVal))
                        resultList.Add(new StatisticRow(++i, item.Key, item.Value));
                } 
            }

            return resultList;
        }


        /// <summary>
        /// Вычитывание содержимого страницы сайта в виде текста
        /// </summary>
        /// <param name="url">Адрес сайта</param>
        /// <param name="dump">Флаг сохранения полученных данных в файл</param>
        /// <returns>Содержимое страницы в виде текста, если в процессе произошла ошибка - возвращается пустая строка.</returns>
        private string getContent(string url, bool dump)
        {
            // Создаем WebClient
            WebClient client = new WebClient();
            //client.Headers.Add("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2");
            string text = "";
            try
            {
                // конвертируем строку в Uri адрес
                if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri rightUri))
                {
                    logger("Не удаось конвертировать указанный адрес \"" + url + "\" в корректный формат Uri");
                    return "";
                }

                // Загружаем через клиент содержимое сайта
                text = client.DownloadString(rightUri);
            }
            catch (Exception e)
            {
                logger("Сбой при получении контента по указанному адресу: \"" + url + "\". Исключение = " + e.Message);
                text = "";
            }

            if (dump && text != "")
            {
                try
                {
                    File.WriteAllText("CurSiteHtmlDump.html", text);
                }
                catch (Exception e)
                {
                    logger("Сбой при сохранении дампа страницы сайта (\"" + url + "\") в файл. Исключение = " + e.Message);
                }
            }

            return text;
        }


        /// <summary>
        /// Получение текстовой информации со страницы
        /// </summary>
        /// <param name="htmlText">HTML текст страницы</param>
        /// <param name="dump">Флаг сохранения обрабатываемых данных в файл</param>
        /// <returns>Отображаемый текст со страницы</returns>
        private List<string> GetAllTexts(string htmlText, bool dump)
        {
            List<string> allText = new List<string>();

            HtmlParser parser = new HtmlParser();
            try
            {
                AngleSharp.Html.Dom.IHtmlDocument document = parser.ParseDocument(htmlText);

                // перебираем элементы страницы
                // заголовок
                allText.Add(document.Title);
                // текст ссылок
                foreach (IElement element in document.QuerySelectorAll("a"))
                {
                    allText.Add(element.Text().Trim());
                }
                // текст
                foreach (IElement element in document.QuerySelectorAll("p"))
                {
                    allText.Add(element.Text().Trim());
                }
            }
            catch (Exception e)
            {
                logger("Сбой при анализе/парсинге html текста страницы сайта. Исключение = " + e.Message);
            }

            if (dump && allText != null)
            {
                try
                {
                    File.WriteAllLines("AllTextHtmlDump.txt", allText);
                }
                catch (Exception e)
                {
                    logger("Сбой при сохранении дампа распарсенного со страницы текста в файл");
                }
            }

            return allText;
        }


        /// <summary>
        /// Парсинг текста на слова и подсчет повторений
        /// </summary>
        /// <param name="strings">Список строк текста</param>
        /// <param name="needRegister">Флаг сравнения слов, с учетом регистра букв</param>
        /// <returns>Словарь уникальных слов из текста с количеством повторений (Ключ - слово, Значение - кол-во повторений)</returns>
        private Dictionary<string, int> parseStringsToWords (List<string> strings, bool needRegister)
        {
            Dictionary<string, int> words = new Dictionary<string, int>();

            foreach (var curString in strings)
            {
                var newWords = curString.Split(splitters);
                foreach (var oneWord in newWords)
                {
                    if (oneWord != "")
                    {
                        string item = oneWord;
                        if (needRegister)
                            item = item.ToUpper();

                        if (words.ContainsKey(item))
                        {
                            words[item]++;
                        }
                        else
                        {
                            words.Add(item, 1);
                        }
                    }
                }
            }

            return words;
        }






    }
}
