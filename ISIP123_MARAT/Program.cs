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
// Главный менеджер университета
public class UniversityManager
{
    private List<Student> _students;
    private List<Teacher> _Teachers;
    private List<Course> _courses;

    public UniversityManager()
    {
        _students = new List<Student>();
        _Teachers = new List<Teacher>();
        _courses = new List<Course>();

        // Добавляем тестовые данные для демонстрации
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        // Создаем преподавателей
        AddTeacher("Иванов Петр Сергеевич", 45, "ivanov@university.ru", "Информатика");
        AddTeacher("Сидорова Мария Ивановна", 38, "sidorova@university.ru", "Математика");

        // Создаем студентов
        AddStudent("Петров Алексей", 20, "petrov@student.ru", "Компьютерные науки");
        AddStudent("Козлова Анна", 19, "kozlova@student.ru", "Математика");

        // Создаем курсы
        AddCourse("Программирование на C#", "Основы программирования на C#");
        AddCourse("Высшая математика", "Математический анализ");

        // Назначаем преподавателей на курсы
        AssignTeacherToCourse(1, 1); // Иванов на Программирование
        AssignTeacherToCourse(2, 2); // Сидорова на Математику

        // Записываем студентов на курсы
        EnrollStudentInCourse(1, 1); // Петров на Программирование
        EnrollStudentInCourse(2, 2); // Козлова на Математику
    }

    // Методы для работы со студентами
    public void AddStudent(string name, int age, string email, string major)
    {
        var studentId = _students.Count + 1;
        var student = new Student(studentId, name, age, email, major);
        _students.Add(student);
    }

    public Student GetStudentById(int id)
    {
        foreach (var student in _students)
        {
            if (student.PersonId == id)
                return student;
        }
        return null;
    }

    public List<Student> GetAllStudents() => _students;
    // Методы для работы с преподавателями
    public void AddTeacher(string name, int age, string email, string department)
    {
        var TeacherId = _Teachers.Count + 1;
        var Teacher = new Teacher(TeacherId, name, age, email, department);
        _Teachers.Add(Teacher);
    }

    public Teacher GetTeacherById(int id)
    {
        foreach (var Teacher in _Teachers)
        {
            if (Teacher.PersonId == id)
                return Teacher;
        }
        return null;
    }

    public List<Teacher> GetAllTeachers() => _Teachers;

    // Методы для работы с курсами
    public void AddCourse(string courseName, string description)
    {
        var courseId = _courses.Count + 1;
        var course = new Course(courseId, courseName, description);
        _courses.Add(course);
    }

    public Course GetCourseById(int id)
    {
        foreach (var course in _courses)
        {
            if (course.CourseId == id)
                return course;
        }
        return null;
    }

    public List<Course> GetAllCourses() => _courses;

    // Методы для связывания сущностей
    public bool EnrollStudentInCourse(int studentId, int courseId)
    {
        var student = GetStudentById(studentId);
        var course = GetCourseById(courseId);

        if (student != null && course != null)
        {
            student.EnrollInCourse(course);
            return true;
        }
        return false;
    }

    public bool AssignTeacherToCourse(int TeacherId, int courseId)
    {
        var Teacher = GetTeacherById(TeacherId);
        var course = GetCourseById(courseId);

        if (Teacher != null && course != null)
        {
            Teacher.AssignToCourse(course);
            return true;
        }
        return false;
    }

    // Методы для получения информации
    public string GetStudentCoursesInfo(int studentId)
    {
        var student = GetStudentById(studentId);
        return student?.GetCourseList() ?? "Студент не найден";
    }

    public string GetCourseStudentsInfo(int courseId)
    {
        var course = GetCourseById(courseId);
        return course?.GetStudentList() ?? "Курс не найден";
    }
}