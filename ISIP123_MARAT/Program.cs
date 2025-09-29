//практика 3

using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;



class Program
{
    class StatText
    {
        public string Text { get; set; }
        public int WordCount { get; set; }
        public string ShortWord { get; set; }
        public string LongWord { get; set; }
        public int SentencesCount { get; set; }
        public int GlasCount { get; set; }
        public int SoglasCount { get; set; }
        public Dictionary<char, int> LetterFrequency { get; set; } // char - буквы, int - их кол-во
        public DateTime AnalysisDate { get; set; } // для просмотра анализа прошлых текстов

        public StatText()
        {
            LetterFrequency = new Dictionary<char, int>();
        }
    }
    static List<StatText>
        allStat = new List<StatText>();

    static char[] Glas = { 'а', 'e', 'ё', 'и', 'о', 'ы', 'у', 'э', 'ю', 'я' };
    static char[] SentencesCount = { '.', '?', '!', ';' };

    static void Main(string[] args)
    {
        Console.WriteLine("--анализ текста--");
        bool ContWork = true;
        while (ContWork)
        {
            ShowMainMenu();
            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    AnalyzeNewText;
                    break;
                case "2":
                    ShowAllStat;
                    break;
                case "3":
                    ContWork = false;
                    Console.WriteLine("конец программы");
                    break;
                default:
                    Console.WriteLine("ошибка");
                    break;
            }
        }
    }
    static void ShowMainMenu()
    {
        Console.WriteLine("--Главное меню--");
        Console.WriteLine("1. анализировать новый текст");
        Console.WriteLine("2. показать статистику по прошлым текстам");
        Console.WriteLine("3. выйти");
        Console.Write("выбрать цифру: ");
    }
    static void AnalyzeNewText()
    {
        Console.WriteLine("--Анализ нового текста--");
        string text;
        while (true)
        {
            Console.WriteLine("введите текст (мин 100 символов");
            text = Console.ReadLine();
            
            if (text == null || text.Length < 100) // Проверяем, что текст не null и содержит достаточно символов
            {
                Console.WriteLine($"Текст должен содержать минимум 100 символов. Сейчас: {text?.Length ?? 0} символов.");
            }
            else
            {
                break;
            }
        }
    }
    }

}

