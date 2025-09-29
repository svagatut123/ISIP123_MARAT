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
            public string Text { get; set; }
            
            public int WordCount { get; set; }
            // Количество слов в тексте
            public string ShortestWord { get; set; }
            // Самое короткое слово
            public string LongestWord { get; set; }
            // Самое длинное слово
            public int SentenceCount { get; set; }
            // Количество предложений
            public int VowelCount { get; set; }
            // Количество гласных букв
            public int ConsonantCount { get; set; }
            // Количество согласных букв 
            public Dictionary<char, int> LetterFrequency { get; set; }
            // словарь для хранения частоты каждой буквы. get - буква, set - сколько раз встретилась
            public DateTime AnalysisDate { get; set; }
            // дата и время проведения анализа


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

            Console.WriteLine("=== анализатор текста ===");

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
                        Console.WriteLine("до свидания!");
                        break;
                    default:
                        Console.WriteLine("неверный выбор. попробуйте снова.");
                        break;
                }
            }
        }

        static void ShowMainMenu()
        {
            Console.WriteLine("\n=== главное меню ===");
            Console.WriteLine("1. анализировать новый текст");
            Console.WriteLine("2. показать статистику по прошлым текстам");
            Console.WriteLine("3. выйти");
            Console.Write("выберите действие: ");
        }


        static void AnalyzeNewText()
    {
        Console.WriteLine("\n=== анализ нового текста ===");

        string text;
        while (true)
        {
            Console.WriteLine("введите текст (минимум 100 символов):");
            text = Console.ReadLine();

            if (text == null || text.Length < 100)
            {
                Console.WriteLine($"текст должен содержать минимум 100 символов. сейчас: {text?.Length ?? 0} символов.");
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

        // подсчитываем количество слов и сохраняем результат
        stats.WordCount = CountWords(text);

        // находим самое короткое и самое длинное слово
        FindShortestAndLongestWords(text, stats);

        // подсчитываем количество предложений
        stats.SentenceCount = LongestWords(text);

        // подсчитываем гласные и согласные буквы
        CountVowelsAndConsonants(text, stats);

        // анализируем частоту встречаемости каждой буквы
        AnalyzeLetterFrequency(text, stats);
    }
        // подсчет слов
        static int CountWords(string text)
        {
            // счетчик слов
            int WordCount = 0;
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
                    WordCount++;
                    inWord = true;
                }
            }

            return WordCount;
        }

        //поиск самого короткого и длинного слова
        static void FindShortestAndLongestWords(string text, TextStatistics stats)
        {
            string ShortestWord = null;
            string LongestWord = null;
            StringBuilder currentWord = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (IsWordSeparator(c))
                {
                    if (currentWord.Length > 0)
                    {
                        string word = currentWord.ToString();
                        // обновляем самое короткое и самое длинное слово
                        UpdateShortestAndLongest(word, ref ShortestWord, ref LongestWord);
                        // очищаем StringBuilder для следующего слова
                        currentWord.Clear();
                    }
                }
                else
                {
                    // если это не разделитель, добавляем символ к текущему слову
                    currentWord.Append(c);
                }
            }

            // обрабатываем последнее слово, если текст не заканчивается разделителем
            if (currentWord.Length > 0)
            {
                string word = currentWord.ToString();
                UpdateShortestAndLongest(word, ref ShortestWord, ref LongestWord);
            }

            stats.ShortestWord = ShortestWord ?? "";
            stats.LongestWord = LongestWord ?? "";
        }

        static void UpdateShortestAndLongest(string word, ref string shortest, ref string longest)
        {
            if (shortest == null || word.Length < shortest.Length)
            {
                shortest = word;
            }

            if (longest == null || word.Length > longest.Length)
            {
                longest = word;
            }
        }

        // кол-во предложений
        static int LongestWords(string text)
        {
            // счетчик предложений
            int SentenceCount = 0;
            bool inSentence = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // проверяем, является ли символ концом предложения
                if (IsSentenceEnding(c))
                {
                    if (inSentence)
                    {
                        SentenceCount++; // увеличиваем счетчик предложений
                        inSentence = false;
                    }
                }
                else if (char.IsLetter(c) && !inSentence)
                {
                    inSentence = true; // если нашли букву и не были в предложении - значит началось новое предложение
                }
            }

            // учитываем последнее предложение, если текст не заканчивается точкой
            if (inSentence)
            {
                SentenceCount++;
            }

            return SentenceCount;
        }

        // метод для подсчета гласных и согласных букв
        static void CountVowelsAndConsonants(string text, TextStatistics stats)
        {
            // счетчики гласных и согласных
            int vowels = 0;
            int consonants = 0;

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    char lowerC = char.ToLower(c);
                    if (IsVowel(lowerC))
                    {
                        vowels++;
                    }
                    else
                    {
                        consonants++;
                    }
                }
            }

            stats.VowelCount = vowels;
            stats.ConsonantCount = consonants;
        }

        // метод для анализа частоты встречаемости каждой буквы
        static void AnalyzeLetterFrequency(string text, TextStatistics stats)
        {
            stats.LetterFrequency.Clear();

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    // приводим букву к нижнему регистру для унификации
                    char lowerC = char.ToLower(c);

                    // проверяем, есть ли уже такая буква в словаре
                    if (stats.LetterFrequency.ContainsKey(lowerC))
                    {
                        stats.LetterFrequency[lowerC]++;
                    }
                    else
                    {
                        stats.LetterFrequency[lowerC] = 1;
                    }
                }
            }
        }

