using System;
using System.Collections.Generic;
using System.Text;

namespace CounterHTML
{
    /// <summary>
    /// Модель строки, со статистикой по одному слову
    /// </summary>
    class StatisticRow : BaseVM
    {
        private int _Num;
        /// <summary>
        /// Номер строки\найденного слова
        /// </summary>
        public int Num
        {
            get => _Num;
            set
            {
                _Num = value;
                OnPropertyChanged();
            }
        }


        private string _fWord;
        /// <summary>
        /// Само найденное слово
        /// </summary>
        public string fWord
        {
            get => _fWord;
            set
            {
                _fWord = value;
                OnPropertyChanged();
            }
        }


        private int _cntWord;
        /// <summary>
        /// Количество этого слова на странице
        /// </summary>
        public int cntWord
        {
            get => _cntWord;
            set
            {
                _cntWord = value;
                OnPropertyChanged();
            }
        }



        /// <summary>
        /// Конструктор экземпляра класса описывающего одну строку списка
        /// </summary>
        /// <param name="num">Номер по порядку</param>
        /// <param name="word">Слово, найденное на странице</param>
        /// <param name="cnt">Количество упоминаний данного слова</param>
        public StatisticRow(int num, string word, int cnt)
        {
            Num = num;
            fWord = word;
            cntWord = cnt;
        }

    }
}
