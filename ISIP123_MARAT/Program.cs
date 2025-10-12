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
    public List<Course> Courses => _courses;
    public string Major => _major;

    // Полиморфизм - реализация абстрактного метода
    public override string GetRole() => "Студент";

    public override string GetInfo()
    {
        return base.GetInfo() + $", Специальность: {_major}";
    }

    public void EnrollInCourse(Course course)
    {
        if (!_courses.Contains(course))
        {
            _courses.Add(course);
            course.AddStudent(this);
        }
    }

    public string GetCourseList()
    {
        if (_courses.Count == 0)
            return "Студент не записан на курсы";

        var courseNames = "";
        foreach (var course in _courses)
        {
            courseNames += $"- {course.CourseName}\n";
        }
        return courseNames;
    }
}

// Класс преподавателя
public class Teacher : Person
{
    private string _department;
    private List<Course> _teachingCourses;

    public Teacher(int TeacherId, string name, int age, string email, string department)
        : base(TeacherId, name, age, email)
    {
        _department = department;
        _teachingCourses = new List<Course>();
    }

    public string Department => _department;
    public List<Course> TeachingCourses => _teachingCourses;

    // Полиморфизм - своя реализация метода
    public override string GetRole() => "Преподаватель";

    public override string GetInfo()
    {
        return base.GetInfo() + $", Кафедра: {_department}";
    }

    public void AssignToCourse(Course course)
    {
        if (!_teachingCourses.Contains(course))
        {
            _teachingCourses.Add(course);
            course.AssignTeacher(this);
        }
    }
}

// Класс курса
public class Course
{
    private int _courseId;
    private string _courseName;
    private string _description;
    private Teacher _Teacher;
    private List<Student> _enrolledStudents;

    public Course(int courseId, string courseName, string description)
    {
        _courseId = courseId;
        _courseName = courseName;
        _description = description;
        _enrolledStudents = new List<Student>();
        _Teacher = null;
    }

    // Свойства для доступа к данным
    public int CourseId => _courseId;
    public string CourseName => _courseName;
    public string Description => _description;
    public Teacher Teacher => _Teacher;
    public List<Student> EnrolledStudents => _enrolledStudents;

    public string GetInfo()
    {
        string TeacherInfo = _Teacher != null ? _Teacher.Name : "Не назначен";
        return $"Курс: {_courseName}\nОписание: {_description}\n" +
               $"Преподаватель: {TeacherInfo}\n" +
               $"Студентов: {_enrolledStudents.Count}";
    }

    public void AssignTeacher(Teacher Teacher)
    {
        _Teacher = Teacher;
    }

    public void AddStudent(Student student)
    {
        if (!_enrolledStudents.Contains(student))
        {
            _enrolledStudents.Add(student);
        }
    }

    public string GetStudentList()
    {
        if (_enrolledStudents.Count == 0)
            return "На курс не записаны студенты";

        var studentNames = "";
        foreach (var student in _enrolledStudents)
        {
            studentNames += $"- {student.Name} ({student.Major})\n";
        }
        return studentNames;
    }
}