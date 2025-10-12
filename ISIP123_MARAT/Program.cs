//ПР5


using System;
using System.Collections.Generic;

// Базовый класс для всех людей в университете
public abstract class Person
{
    // Приватные поля - инкапсуляция
    private int _personId;
    private string _name;
    private int _age;
    private string _email;

    // Protected конструктор - только для наследников
    protected Person(int personId, string name, int age, string email)
    {
        // Присваиваем значения полям - без этого объект будет пустым
        _personId = personId;
        _name = name;
        _age = age;
        _email = email;
    }

    // Публичные свойства для доступа к данным
    public int PersonId => _personId;
    public string Name => _name;
    public int Age => _age;
    public string Email => _email;

    public abstract string GetRole(); // Абстрактный метод - полиморфизм

    // Виртуальный метод - можно переопределить в наследниках
    public virtual string GetInfo()
    {
        return $"ID: {_personId}, Имя: {_name}, Возраст: {_age}";
    }
}

// Класс студента
public class Student : Person
{
    private List<Course> _courses;
    private string _major;

    public Student(int studentId, string name, int age, string email, string major)
        : base(studentId, name, age, email) // Вызов конструктора базового класса
    {
        _courses = new List<Course>();
        _major = major;
    }