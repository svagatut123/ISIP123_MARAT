//пр4

using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagement
{
    // Перечисление
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
        private static int _nextId = 1; // Статическая переменная для автоматической генерации ID

        // Свойства книги
        public int Id { get; private set; }         // Уникальный идентификатор
        public string Title { get; set; }           // Название
        public string Author { get; set; }          // Автор
        public Genre Genre { get; set; }            // Жанр
        public int Year { get; set; }               // Год издания
        public decimal Price { get; set; }          // Цена

        // Конструктор книги с валидацией
        public Book(string title, string author, Genre genre, int year, decimal price)
        {
            // Проверка на пустое название
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Название книги не может быть пустым");

            // Проверка на пустого автора
            if (string.IsNullOrWhiteSpace(author))
                throw new ArgumentException("Автор не может быть пустым");

            // Проверка корректности года
            if (year < 1000 || year > DateTime.Now.Year)
                throw new ArgumentException($"Год издания должен быть между 1000 и {DateTime.Now.Year}");

            // Проверка на отрицательную цену
            if (price < 0)
                throw new ArgumentException("Цена не может быть отрицательной");

            // Инициализация свойств
            Id = _nextId++;                 // Автоматическое назначение ID
            Title = title.Trim();           // Удаление пробелов в начале/конце
            Author = author.Trim();         // Удаление пробелов в начале/конце
            Genre = genre;                  // Установка жанра
            Year = year;                    // Установка года
            Price = price;                  // Установка цены
        }
        // вывод информации о книге
        public override string ToString()
        {
            return $"ID: {Id}, Название: \"{Title}\", Автор: {Author}, Жанр: {Genre}, Год: {Year}, Цена: {Price:C}";
        }
    }

    public class Library
    {
        private List<Book> _books = new List<Book>(); // список всех книг

        // Свойство для доступа к книгам только для чтения
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

        // Поиск книг по названию
        public IEnumerable<Book> FindBooksByTitle(string title)
        {
            return _books.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        // Поиск книг по автору
        public IEnumerable<Book> FindBooksByAuthor(string author)
        {
            return _books.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
        }

        // Поиск книг по жанру
        public IEnumerable<Book> FindBooksByGenre(Genre genre)
        {
            return _books.Where(b => b.Genre == genre);
        }

        // Общий поиск книг по названию или автору
        public IEnumerable<Book> FindBooks(string searchTerm)
        {
            return _books.Where(b =>
                b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        // Сортировка книг по названию
        public IEnumerable<Book> SortByTitle()
        {
            return _books.OrderBy(b => b.Title);
        }

        // Сортировка книг по году издания
        public IEnumerable<Book> SortByYear()
        {
            return _books.OrderBy(b => b.Year);
        }

        // Сортировка книг по году издания (по убыванию)
        public IEnumerable<Book> SortByYearDescending()
        {
            return _books.OrderByDescending(b => b.Year);
        }

        // Поиск самой дорогой книги
        public Book GetMostExpensiveBook()
        {
            return _books.OrderByDescending(b => b.Price).FirstOrDefault();
        }

        // Поиск самой дешевой книги
        public Book GetCheapestBook()
        {
            return _books.OrderBy(b => b.Price).FirstOrDefault();
        }
        // Сгруппировать книги по авторам и вывести количество книг каждого автора.
        public void DisplayBooksByAuthors()
        {
            // Группировка по автору и подсчет количества книг
            var booksByAuthor = _books.GroupBy(b => b.Author)
                                      .Select(g => new { Author = g.Key, Count = g.Count() })
                                      .OrderByDescending(g => g.Count);

            Console.WriteLine("\nКоличество книг по авторам:");
            Console.WriteLine(new string('-', 40));

            foreach (var group in booksByAuthor)
            {
                Console.WriteLine($"Автор: {group.Author}, Количество книг: {group.Count}");
            }
        }
    }
}
