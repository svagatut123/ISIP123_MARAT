//практика 3

using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;



namespace TextAnalyzer
{
    class Program
    {
        class TextStatistics
        {
            public string Text { get; set; } // Количество слов в тексте
            public int ShortWord { get; set; } // Самое короткое слово
            public string LongWord { get; set; } // Самое длинное слово
            public string CountSentence { get; set; }// Количество предложений
            public int GlasSentence { get; set; } // Количество гласных букв
            public int VowelCount { get; set; } // Количество согласных букв
            public int SoglasCount { get; set; } // Словарь для хранения частоты каждой буквы. get - буква, set - сколько раз встретилась
            public Dictionary<char, int> LetterFrequency { get; set; } // Дата и время проведения анализа
            public DateTime AnalysisDate { get; set; }

           
            public TextStatistics()
            {
                // словарь для частоты букв
                LetterFrequency = new Dictionary<char, int>();
            }
        }

        // список для хранения статистики по всем текстам
        static List<TextStatistics> allStatistics = new List<TextStatistics>();

        
        static char[] vowels = { 'а', 'е', 'ё', 'и', 'о', 'у', 'ы', 'э', 'ю', 'я' };
        static char[] sentenceEndings = { '.', '!', '?' };
        static char[] wordSeparators = { ' ', ',', ';', ':', '-', '\n', '\r', '\t' }; //чтобы слова через разделитель считались за 1 слово (например когда-либо)

        static void Main(string[] args)
        {

            Console.WriteLine("=== Анализатор текста ===");

            bool continueWorking = true;
            while (continueWorking)
            {
                ShowMainMenu();
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AnalyzeNewText();
                        break;
                    case "2":
                        ShowAllStatistics();
                        break;
                    case "3":
                        continueWorking = false;
                        Console.WriteLine("До свидания!");
                        break;
                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        break;
                }
            }
        }

        static void ShowMainMenu()
        {
            Console.WriteLine("\n=== Главное меню ===");
            Console.WriteLine("1. Анализировать новый текст");
            Console.WriteLine("2. Показать статистику по прошлым текстам");
            Console.WriteLine("3. Выйти");
            Console.Write("Выберите действие: ");
        }


        static void AnalyzeNewText()
    {
        Console.WriteLine("\n=== Анализ нового текста ===");

        string text;
        while (true)
        {
            Console.WriteLine("Введите текст (минимум 100 символов):");
            text = Console.ReadLine();

            if (text == null || text.Length < 100)
            {
                Console.WriteLine($"Текст должен содержать минимум 100 символов. Сейчас: {text?.Length ?? 0} символов.");
            }
            else
            {
                break;
            }
        }

        //новый объект для хранения статистики
        TextStatistics stats = new TextStatistics();
        // сохранение текста
        stats.Text = text;
        stats.AnalysisDate = DateTime.Now;

        AnalyzeText(stats);

        allStatistics.Add(stats);

        ShowCurrentStatistics(stats);
    }

    static void AnalyzeText(TextStatistics stats)
    {
        string text = stats.Text;

        // Подсчитываем количество слов и сохраняем результат
        stats.ShortWord = CountWords(text);

        // Находим самое короткое и самое длинное слово
        FindShortestAndCountSentences(text, stats);

        // Подсчитываем количество предложений
        stats.GlasSentence = CountSentences(text);

        // Подсчитываем гласные и согласные буквы
        CountVowelsAndConsonants(text, stats);

        // Анализируем частоту встречаемости каждой буквы
        AnalyzeLetterFrequency(text, stats);
    }
        // подсчет слов
        static int CountWords(string text)
        {
            // счетчик слов
            int ShortWord = 0;
            bool inWord = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (IsWordSeparator(c))
                {
                    inWord = false;
                }
                else if (!inWord)
                {
                    ShortWord++;
                    inWord = true;
                }
            }

            return ShortWord;
        }


