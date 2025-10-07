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
    }
}
