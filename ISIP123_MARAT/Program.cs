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

        // метод для отображения статистики по текущему тексту
        static void ShowCurrentStatistics(TextStatistics stats)
        {
            Console.WriteLine("\n=== результаты анализа ===");
            Console.WriteLine($"дата анализа: {stats.AnalysisDate}");
            Console.WriteLine($"общее количество символов: {stats.Text.Length}");
            Console.WriteLine($"количество слов: {stats.WordCount}");
            Console.WriteLine($"самое короткое слово: \"{stats.ShortestWord}\" (длина: {stats.ShortestWord.Length})");
            Console.WriteLine($"самое длинное слово: \"{stats.LongestWord}\" (длина: {stats.LongestWord.Length})");
            Console.WriteLine($"количество предложений: {stats.SentenceCount}");
            Console.WriteLine($"количество гласных букв: {stats.VowelCount}");
            Console.WriteLine($"количество согласных букв: {stats.ConsonantCount}");

            Console.WriteLine("\nстатистика по буквам:");
            if (stats.LetterFrequency.Count > 0)
            {
                // создаем список букв из ключей словаря
                List<char> letters = new List<char>(stats.LetterFrequency.Keys);

                // сортируем буквы по убыванию (пузырьковая сортировка)
                for (int i = 0; i < letters.Count - 1; i++)
                {
                    for (int j = 0; j < letters.Count - i - 1; j++)
                    {
                        // сравниваем частоты двух соседних букв
                        if (stats.LetterFrequency[letters[j]] < stats.LetterFrequency[letters[j + 1]])
                        {
                            // меняем буквы местами, если частота текущей меньше следующей
                            char temp = letters[j];
                            letters[j] = letters[j + 1];
                            letters[j + 1] = temp;
                        }
                    }
                }

                // выводим отсортированный список букв с частотами
                foreach (char letter in letters)
                {
                    // получаем частоту текущей буквы
                    int frequency = stats.LetterFrequency[letter];
                    // вычисляем процентное соотношение от общего количества букв
                    double percentage = (double)frequency / (stats.VowelCount + stats.ConsonantCount) * 100;
                    // выводим информацию о букве
                    Console.WriteLine($"  {letter}: {frequency} раз ({percentage:F2}%)");
                }
            }
            else
            {
                Console.WriteLine("  буквы не найдены");
            }
        }

        // метод для отображения статистики по всем проанализированным текстам
        static void ShowAllStatistics()
        {
            Console.WriteLine("\n=== статистика по всем текстам ===");

            if (allStatistics.Count == 0)
            {
                Console.WriteLine("статистика отсутствует. сначала проанализируйте текст.");
                return;
            }

            for (int i = 0; i < allStatistics.Count; i++)
            {
                TextStatistics stats = allStatistics[i]; // вывод заголовка для текущего анализа
                Console.WriteLine($"\n--- анализ #{i + 1} ({stats.AnalysisDate}) ---"); // показываем превью текста
                Console.WriteLine($"текст: {GetTextPreview(stats.Text)}");// вывод осн. показателей
                Console.WriteLine($"слов: {stats.WordCount}, предложений: {stats.SentenceCount}");
                Console.WriteLine($"гласные: {stats.VowelCount}, согласные: {stats.ConsonantCount}");
                Console.WriteLine($"самое короткое слово: \"{stats.ShortestWord}\"");
                Console.WriteLine($"самое длинное слово: \"{stats.LongestWord}\"");
            }

            // выводим сводную статистику по всем анализам
            Console.WriteLine("\n=== сводная статистика ===");
            Console.WriteLine($"всего проанализировано текстов: {allStatistics.Count}");

            // если есть анализы, вычисляем общие показатели
            if (allStatistics.Count > 0)
            {
                int totalWords = 0;
                int totalSentences = 0;
                int totalVowels = 0;
                int totalConsonants = 0;

                // проходим по всем анализам и суммируем показатели
                foreach (TextStatistics stats in allStatistics)
                {
                    totalWords += stats.WordCount;
                    totalSentences += stats.SentenceCount;
                    totalVowels += stats.VowelCount;
                    totalConsonants += stats.ConsonantCount;
                }

                // выводим суммарные показатели
                Console.WriteLine($"общее количество слов: {totalWords}");
                Console.WriteLine($"общее количество предложений: {totalSentences}");
                Console.WriteLine($"общее количество гласных: {totalVowels}");
                Console.WriteLine($"общее количество согласных: {totalConsonants}");
                // вычисляем и выводим средние значения
                Console.WriteLine($"среднее количество слов на текст: {totalWords / allStatistics.Count}");
            }
        }

        //метод для проверки, является ли символ разделителем слов
        static bool IsWordSeparator(char c)
        {
            // проходим по всем разделителям
            foreach (char separator in wordSeparators)
            {
                if (c == separator)
                    return true;
            }
            return false;
        }

        //метод для проверки, является ли символ концом предложения
        static bool IsSentenceEnding(char c)
        {
            // проходим по всем знакам конца предложения
            foreach (char ending in sentenceEndings)
            {
                if (c == ending)
                    return true;
            }
            return false;
        }
