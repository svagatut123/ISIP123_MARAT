//пр4

using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagement
{
    // перечисление
    public enum Genre
    {
        Fiction = 1,
        Science = 2,
        Fantasy = 3,
        Mystery = 4,
        Romance = 5,
        Biography = 6
    }
    public class Book
    {
        private static int _nextId = 1; // статическая переменная для автоматической генерации ID

        // свойства книги
        public int Id { get; private set; }         // уникальный идентификатор
        public string Title { get; set; }           // название
        public string Author { get; set; }          // автор
        public Genre Genre { get; set; }            // жанр
        public int Year { get; set; }               // год издания
        public decimal Price { get; set; }          // цена

        // конструктор книги с валидацией
        public Book(string title, string author, Genre genre, int year, decimal price)
        {
            // проверка на пустое название
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("название книги не может быть пустым");

            // проверка на пустого автора
            if (string.IsNullOrWhiteSpace(author))
                throw new ArgumentException("автор не может быть пустым");

            // проверка корректности года
            if (year < 1000 || year > DateTime.Now.Year)
                throw new ArgumentException($"год издания должен быть между 1000 и {DateTime.Now.Year}");

            // проверка на отрицательную цену
            if (price < 0)
                throw new ArgumentException("цена не может быть отрицательной");

            // инициализация свойств
            Id = _nextId++;                 // автоматическое назначение ID
            Title = title.Trim();           // удаление пробелов в начале/конце
            Author = author.Trim();         // удаление пробелов в начале/конце
            Genre = genre;                  // установка жанра
            Year = year;                    // установка года
            Price = price;                  // установка цены
        }
        // вывод информации о книге
        public override string ToString()
        {
            return $"ID: {Id}, название: \"{Title}\", автор: {Author}, жанр: {Genre}, год: {Year}, цена: {Price:C}";
        }
    }

    public class Library
    {
        private List<Book> _books = new List<Book>(); // список всех книг

        // свойство для доступа к книгам только для чтения
        public IReadOnlyList<Book> Books => _books.AsReadOnly();

        //добавление книги
        public void AddBook(Book book)
        {
            _books.Add(book); // добавление книги в список
        }

        //удаление книги по id
        public bool RemoveBook(int id)
        {
            //поиск книги по id
            var book = _books.FirstOrDefault(b => b.Id == id);
            if (book != null)
            {
                _books.Remove(book);
                return true;
            }
            return false;
        }

        // поиск книг по названию
        public IEnumerable<Book> FindBooksByTitle(string title)
        {
            return _books.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        // поиск книг по автору
        public IEnumerable<Book> FindBooksByAuthor(string author)
        {
            return _books.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
        }

        // поиск книг по жанру
        public IEnumerable<Book> FindBooksByGenre(Genre genre)
        {
            return _books.Where(b => b.Genre == genre);
        }

        // общий поиск книг по названию или автору
        public IEnumerable<Book> FindBooks(string searchTerm)
        {
            return _books.Where(b =>
                b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        // сортировка книг по названию
        public IEnumerable<Book> SortByTitle()
        {
            return _books.OrderBy(b => b.Title);
        }

        // сортировка книг по году издания
        public IEnumerable<Book> SortByYear()
        {
            return _books.OrderBy(b => b.Year);
        }

        // сортировка книг по году издания (по убыванию)
        public IEnumerable<Book> SortByYearDescending()
        {
            return _books.OrderByDescending(b => b.Year);
        }

        // поиск самой дорогой книги
        public Book GetMostExpensiveBook()
        {
            return _books.OrderByDescending(b => b.Price).FirstOrDefault();
        }

        // поиск самой дешевой книги
        public Book GetCheapestBook()
        {
            return _books.OrderBy(b => b.Price).FirstOrDefault();
        }
        // сгруппировать книги по авторам и вывести количество книг каждого автора.
        public void DisplayBooksByAuthors()
        {
            // группировка по автору и подсчет количества книг
            var booksByAuthor = _books.GroupBy(b => b.Author)
                                      .Select(g => new { Author = g.Key, Count = g.Count() })
                                      .OrderByDescending(g => g.Count);

            Console.WriteLine("\nколичество книг по авторам:");
            Console.WriteLine(new string('-', 40));

            foreach (var group in booksByAuthor)
            {
                Console.WriteLine($"автор: {group.Author}, количество книг: {group.Count}");
            }
        }
        // тест данные
        public void InitializeTestData()
        {
            try
            {
                AddBook(new Book("Война и мир", "Лев Толстой", Genre.Fiction, 1869, 1200));
                AddBook(new Book("Преступление и наказание", "Федор Достоевский", Genre.Fiction, 1866, 950));
                AddBook(new Book("Бедная Лиза", "Николай Карамзин", Genre.Science, 1949, 800));
                AddBook(new Book("Горе от ума", "Александр Грибоедов", Genre.Fantasy, 1997, 700));
                AddBook(new Book("Мастер и Маргарита", "Михаил Булгаков", Genre.Fiction, 1967, 1100));

            }
            catch (Exception ex)
            {
                Console.WriteLine($"ошибка при добавлении тестовых данных: {ex.Message}");
            }
        }
        
// главный класс программы
class Program
            {
                // метод для отображения списка книг
                static void DisplayBooks(IEnumerable<Book> books, string message = "результаты:")
                {
                    Console.WriteLine($"\n{message}");
                    Console.WriteLine(new string('-', 80));

                    var bookList = books.ToList();
                    if (!bookList.Any())
                    {
                        Console.WriteLine("книги не найдены.");
                        return;
                    }

                    foreach (var book in bookList)
                    {
                        Console.WriteLine(book);
                    }
                    Console.WriteLine($"всего: {bookList.Count} книг");
                }

                // главный метод программы
                static void Main(string[] args)
                {
                    Library library = new Library();
                    library.InitializeTestData();

                    Console.WriteLine("добро пожаловать в систему учета книг библиотеки!");

                    while (true)
                    {
                        Console.WriteLine("\nглавное меню ");
                        Console.WriteLine("1. добавить книгу");
                        Console.WriteLine("2. удалить книгу по ID");
                        Console.WriteLine("3. найти книги");
                        Console.WriteLine("4. отсортировать книги");
                        Console.WriteLine("5. показать самую дорогую и дешевую книгу");
                        Console.WriteLine("6. сгруппировать книги по авторам");
                        Console.WriteLine("7. показать все книги");
                        Console.WriteLine("0. выход");

                        Console.Write("выберите действие: ");
                        int choice = int.Parse(Console.ReadLine());

                        switch (choice)
                        {
                            case 0:
                                Console.WriteLine("конец программы");
                                return;

                            case 1:
                                try
                                {
                                    Console.Write("введите название книги: ");
                                    string title = Console.ReadLine();

                                    Console.Write("введите автора книги: ");
                                    string author = Console.ReadLine();

                                    Console.WriteLine("\nвыберите жанр:");
                                    foreach (var genre in Enum.GetValues(typeof(Genre)))
                                    {
                                        Console.WriteLine($"{(int)genre}. {genre}");
                                    }
                                    Console.Write("введите номер жанра: ");
                                    Genre genreChoice = (Genre)int.Parse(Console.ReadLine());

                                    Console.Write("введите год издания: ");
                                    int year = int.Parse(Console.ReadLine());

                                    Console.Write("введите цену книги: ");
                                    decimal price = decimal.Parse(Console.ReadLine());

                                    Book newBook = new Book(title, author, genreChoice, year, price);
                                    library.AddBook(newBook);
                                    Console.WriteLine($"книга успешно добавлена! ID: {newBook.Id}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"ошибка: {ex.Message}");
                                }
                                break;

                            case 2:
                                Console.Write("введите ID книги для удаления: ");
                                int idToRemove = int.Parse(Console.ReadLine());
                                if (library.RemoveBook(idToRemove))
                                    Console.WriteLine("книга успешно удалена!");
                                else
                                    Console.WriteLine("книга с указанным id не найдена.");
                                break;

                            case 3:
                                Console.WriteLine("\n поиск книг ");
                                Console.WriteLine("1. по названию");
                                Console.WriteLine("2. по автору");
                                Console.WriteLine("3. по жанру");
                                Console.WriteLine("4. общий поиск");

                                Console.Write("выберите тип поиска: ");
                                int searchChoice = int.Parse(Console.ReadLine());

                                switch (searchChoice)
                                {
                                    case 1:
                                        Console.Write("введите название для поиска: ");
                                        string title = Console.ReadLine();
                                        DisplayBooks(library.FindBooksByTitle(title), "результаты поиска по названию:");
                                        break;
                                    case 2:
                                        Console.Write("введите автора для поиска: ");
                                        string author = Console.ReadLine();
                                        DisplayBooks(library.FindBooksByAuthor(author), "результаты поиска по автору:");
                                        break;
                                    case 3:
                                        Console.WriteLine("\nвыберите жанр:");
                                        foreach (var genre in Enum.GetValues(typeof(Genre)))
                                        {
                                            Console.WriteLine($"{(int)genre}. {genre}");
                                        }
                                        Console.Write("введите номер жанра: ");
                                        Genre searchGenre = (Genre)int.Parse(Console.ReadLine());
                                        DisplayBooks(library.FindBooksByGenre(searchGenre), $"результаты поиска по жанру {searchGenre}:");
                                        break;
                                    case 4:
                                        Console.Write("введите текст для поиска: ");
                                        string searchTerm = Console.ReadLine();
                                        DisplayBooks(library.FindBooks(searchTerm), "результаты общего поиска:");
                                        break;
                                }
                                break;

                            case 4:
                                Console.WriteLine("\n сортировка книг ");
                                Console.WriteLine("1. по названию");
                                Console.WriteLine("2. по году издания (по возрастанию)");
                                Console.WriteLine("3. по году издания (по убыванию)");

                                Console.Write("выберите тип сортировки: ");
                                int sortChoice = int.Parse(Console.ReadLine());

                                switch (sortChoice)
                                {
                                    case 1:
                                        DisplayBooks(library.SortByTitle(), "книги отсортированные по названию:");
                                        break;
                                    case 2:
                                        DisplayBooks(library.SortByYear(), "книги отсортированные по году издания:");
                                        break;
                                    case 3:
                                        DisplayBooks(library.SortByYearDescending(), "книги отсортированные по году издания (по убыванию):");
                                        break;
                                }
                                break;

                            case 5:
                                Book mostExpensive = library.GetMostExpensiveBook();
                                Book cheapest = library.GetCheapestBook();

                                Console.WriteLine("\n анализ цен ");
                                if (mostExpensive != null)
                                    Console.WriteLine($"самая дорогая книга: {mostExpensive}");
                                else
                                    Console.WriteLine("в библиотеке нет книг.");

                                if (cheapest != null)
                                    Console.WriteLine($"самая дешевая книга: {cheapest}");
                                else
                                    Console.WriteLine("в библиотеке нет книг.");
                                break;

                            case 6:
                                library.DisplayBooksByAuthors();
                                break;

                            case 7:
                                DisplayBooks(library.Books, "все книги в библиотеке:");
                                break;

                            default:
                                Console.WriteLine("неизвестная команда!");
                                break;
                        }
                    }
                }
            }
        }
    }
}