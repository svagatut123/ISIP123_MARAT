//пр5
using System;
using System.Collections.Generic;

public abstract class Person
{
    // приватные поля - инкапсуляция
    private int _personId;
    private string _name;
    private int _age;
    private string _email;

    protected Person(int personId, string name, int age, string email)
    {
        // присваиваем значения полям - без этого объект будет пустым
        _personId = personId;
        _name = name;
        _age = age;
        _email = email;
    }

    // публичные свойства для доступа к данным
    public int PersonId => _personId;
    public string Name => _name;
    public int Age => _age;
    public string Email => _email;

    public abstract string GetRole(); // абстрактный метод - полиморфизм

    // виртуальный метод - можно переопределить в наследниках
    public virtual string GetInfo()
    {
        return $"ID: {_personId}, имя: {_name}, возраст: {_age}";
    }
}

// класс студента
public class Student : Person
{
    private List<Course> _courses;
    private string _major;

    public Student(int studentId, string name, int age, string email, string major)
        : base(studentId, name, age, email) // вызов конструктора базового класса
    {
        _courses = new List<Course>();
        _major = major;
    }
    public List<Course> Courses => _courses;
    public string Major => _major;

    // полиморфизм - реализация абстрактного метода
    public override string GetRole() => "студент";

    public override string GetInfo()
    {
        return base.GetInfo() + $", специальность: {_major}";
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
            return "студент не записан на курсы";

        var courseNames = "";
        foreach (var course in _courses)
        {
            courseNames += $"- {course.CourseName}\n";
        }
        return courseNames;
    }
}

// класс преподавателя
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

    // полиморфизм - своя реализация метода
    public override string GetRole() => "преподаватель";

    public override string GetInfo()
    {
        return base.GetInfo() + $", кафедра: {_department}";
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

// класс курса
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

    // свойства для доступа к данным
    public int CourseId => _courseId;
    public string CourseName => _courseName;
    public string Description => _description;
    public Teacher Teacher => _Teacher;
    public List<Student> EnrolledStudents => _enrolledStudents;

    public string GetInfo()
    {
        string TeacherInfo = _Teacher != null ? _Teacher.Name : "не назначен";
        return $"курс: {_courseName}\nописание: {_description}\n" +
               $"преподаватель: {TeacherInfo}\n" +
               $"студентов: {_enrolledStudents.Count}";
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
            return "на курс не записаны студенты";

        var studentNames = "";
        foreach (var student in _enrolledStudents)
        {
            studentNames += $"- {student.Name} ({student.Major})\n";
        }
        return studentNames;
    }
}
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

        // тестовые данные
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        
        AddTeacher("гуцалюк саша сашыч", 45, "sasha@university.ru", "информатика");
        AddTeacher("Сашюк гуцал гуцалыч", 38, "guts@university.ru", "математика");

        
        AddStudent("Сашов сашка", 20, "sassha@student.ru", "физра");
        AddStudent("шуриков шура", 19, "shura@student.ru", "математика");


        AddCourse("робототехника");
        AddCourse("вышивка крючком");


        AssignTeacherToCourse(1, 1);
        AssignTeacherToCourse(2, 2);

        EnrollStudentInCourse(1, 1); 
        EnrollStudentInCourse(2, 2); 
    }

    // методы для работы со студентами
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
    // методы для работы с преподавателями
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
    // Новые методы для выбора из списка (добавь в класс UniversityManager)
    public void ShowAllCourses()
    {
        Console.WriteLine("\nДоступные курсы:");
        foreach (var course in _courses)
        {
            Console.WriteLine($"{course.CourseId}. {course.CourseName}");
        }
    }

    public void ShowAllTeachers()
    {
        Console.WriteLine("\nДоступные преподаватели:");
        foreach (var teacher in _Teachers)
        {
            Console.WriteLine($"{teacher.PersonId}. {teacher.Name} - {teacher.Department}");
        }
    }

    public void ShowAllStudents()
    {
        Console.WriteLine("\nСтуденты:");
        foreach (var student in _students)
        {
            Console.WriteLine($"{student.PersonId}. {student.Name} - {student.Major}");
        }
    }

    // Новые методы для связывания через выбор из списка
    public void EnrollStudentInCourseWithSelection()
    {
        ShowAllStudents();
        Console.Write("\nВыберите ID студента: ");
        if (int.TryParse(Console.ReadLine(), out int studentId))
        {
            ShowAllCourses();
            Console.Write("Выберите ID курса: ");
            if (int.TryParse(Console.ReadLine(), out int courseId))
            {
                var student = GetStudentById(studentId);
                var course = GetCourseById(courseId);

                if (student != null && course != null)
                {
                    student.EnrollInCourse(course);
                    Console.WriteLine($"Студент {student.Name} записан на курс {course.CourseName}!");
                }
                else
                {
                    Console.WriteLine("Ошибка: студент или курс не найден.");
                }
            }
        }
    }

    public void AssignTeacherToCourseWithSelection()
    {
        ShowAllTeachers();
        Console.Write("\nВыберите ID преподавателя: ");
        if (int.TryParse(Console.ReadLine(), out int teacherId))
        {
            ShowAllCourses();
            Console.Write("Выберите ID курса: ");
            if (int.TryParse(Console.ReadLine(), out int courseId))
            {
                var teacher = GetTeacherById(teacherId);
                var course = GetCourseById(courseId);

                if (teacher != null && course != null)
                {
                    teacher.AssignToCourse(course);
                    Console.WriteLine($"Преподаватель {teacher.Name} назначен на курс {course.CourseName}!");
                }
                else
                {
                    Console.WriteLine("Ошибка: преподаватель или курс не найден.");
                }
            }
        }
    }
    public List<Teacher> GetAllTeachers() => _Teachers;

    // методы для работы с курсами
    public void AddCourse(string courseName)
    {
        var courseId = _courses.Count + 1;
        var course = new Course(courseId, courseName, "");
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

    // методы для связывания сущностей
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

    // методы для получения информации
    public string GetStudentCoursesInfo(int studentId)
    {
        var student = GetStudentById(studentId);
        return student?.GetCourseList() ?? "студент не найден";
    }

    public string GetCourseStudentsInfo(int courseId)
    {
        var course = GetCourseById(courseId);
        return course?.GetStudentList() ?? "курс не найден";
    }
}
public class UniversityConsoleMenu
{
    private UniversityManager _universityManager;

    public UniversityConsoleMenu()
    {
        _universityManager = new UniversityManager();
    }

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("система управления студентами");
            Console.WriteLine("1. управление студентами");
            Console.WriteLine("2. управление преподавателями");
            Console.WriteLine("3. управление курсами");
            Console.WriteLine("4. просмотр всех данных");
            Console.WriteLine("5. выход");
            Console.Write("выберите опцию: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    ManageStudents();
                    break;
                case "2":
                    ManageTeachers();
                    break;
                case "3":
                    ManageCourses();
                    break;
                case "4":
                    ViewAllData();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("неверный выбор.");
                    WaitForKey();
                    break;
            }
        }
    }

    private void ManageStudents()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("управление студентами");
            Console.WriteLine("1. добавить студента");
            Console.WriteLine("2. просмотреть всех студентов");
            Console.WriteLine("3. просмотреть курсы студента");
            Console.WriteLine("4. записать студента на курс");
            Console.WriteLine("5. назад");
            Console.Write("выберите опцию: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    AddStudent();
                    break;
                case "2":
                    ViewAllStudents();
                    break;
                case "3":
                    ViewStudentCourses();
                    break;
                case "4":
                    EnrollStudentInCourse();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("неверный выбор.");
                    break;
            }
            WaitForKey();
        }
    }

    private void AddStudent()
    {
        Console.Write("введите имя: ");
        var name = Console.ReadLine();
        Console.Write("введите возраст: ");
        var age = int.Parse(Console.ReadLine());
        Console.Write("введите email: ");
        var email = Console.ReadLine();
        Console.Write("введите специальность: ");
        var major = Console.ReadLine();

        _universityManager.AddStudent(name, age, email, major);
        Console.WriteLine("студент успешно добавлен!");
    }

    private void ViewAllStudents()
    {
        var students = _universityManager.GetAllStudents();
        if (students.Count == 0)
        {
            Console.WriteLine("студенты не найдены.");
            return;
        }

        Console.WriteLine("\nсписок всех студентов:");
        foreach (var student in students)
        {
            Console.WriteLine(student.GetInfo());
        }
    }

    private void ViewStudentCourses()
    {
        _universityManager.ShowAllStudents();
        Console.Write("\nвыберите ID студента для просмотра курсов: ");
        if (int.TryParse(Console.ReadLine(), out int studentId))
        {
            var info = _universityManager.GetStudentCoursesInfo(studentId);
            Console.WriteLine($"\nкурсы студента:");
            Console.WriteLine(info);
        }
        else
        {
            Console.WriteLine("некорректный ID.");
        }
    }

    private void EnrollStudentInCourse()
    {
        _universityManager.ShowAllStudents();
        Console.Write("\nвыберите ID студента: ");
        if (int.TryParse(Console.ReadLine(), out int studentId))
        {
            _universityManager.ShowAllCourses();
            Console.Write("выберите ID курса: ");
            if (int.TryParse(Console.ReadLine(), out int courseId))
            {
                if (_universityManager.EnrollStudentInCourse(studentId, courseId))
                    Console.WriteLine("студент успешно записан на курс");
                else
                    Console.WriteLine("студент или курс не найден");
            }
        }
    }

    private void ManageTeachers()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("управление проподами");
            Console.WriteLine("1. добавить преподавателя");
            Console.WriteLine("2. просмотреть всех преподавателей");
            Console.WriteLine("3. назначить преподавателя на курс");
            Console.WriteLine("4. просмотреть курсы преподавателя");
            Console.WriteLine("5. назад");
            Console.Write("выберите опцию: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    AddTeacher();
                    break;
                case "2":
                    ViewAllTeachers();
                    break;
                case "3":
                    AssignTeacherToCourse();
                    break;
                case "4":
                    ViewTeacherCourses();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("неверный выбор.");
                    break;
            }
            WaitForKey();
        }
    }

    private void AddTeacher()
    {
        Console.Write("введите имя: ");
        var name = Console.ReadLine();
        Console.Write("введите возраст: ");
        var age = int.Parse(Console.ReadLine());
        Console.Write("введите email: ");
        var email = Console.ReadLine();
        Console.Write("введите кафедру: ");
        var department = Console.ReadLine();

        _universityManager.AddTeacher(name, age, email, department);
        Console.WriteLine("преподаватель успешно добавлен!");
    }

    private void ViewAllTeachers()
    {
        var teachers = _universityManager.GetAllTeachers();
        if (teachers.Count == 0)
        {
            Console.WriteLine("преподаватели не найдены.");
            return;
        }

        Console.WriteLine("\nсписок всех преподавателей:");
        foreach (var teacher in teachers)
        {
            Console.WriteLine(teacher.GetInfo());
        }
    }

    private void AssignTeacherToCourse()
    {
        _universityManager.ShowAllTeachers();
        Console.Write("\nвыберите ID преподавателя: ");
        if (int.TryParse(Console.ReadLine(), out int teacherId))
        {
            _universityManager.ShowAllCourses();
            Console.Write("выберите ID курса: ");
            if (int.TryParse(Console.ReadLine(), out int courseId))
            {
                if (_universityManager.AssignTeacherToCourse(teacherId, courseId))
                    Console.WriteLine("преподаватель успешно назначен на курс");
                else
                    Console.WriteLine("преподаватель или курс не найден");
            }
        }
    }

    private void ViewTeacherCourses()
    {
        _universityManager.ShowAllTeachers();
        Console.Write("\nвыберите ID преподавателя для просмотра курсов: ");
        if (int.TryParse(Console.ReadLine(), out int teacherId))
        {
            var teacher = _universityManager.GetTeacherById(teacherId);
            if (teacher != null)
            {
                Console.WriteLine($"\nкурсы преподавателя {teacher.Name}:");
                if (teacher.TeachingCourses.Count == 0)
                {
                    Console.WriteLine("преподаватель не ведет курсы");
                }
                else
                {
                    foreach (var course in teacher.TeachingCourses)
                    {
                        Console.WriteLine($"- {course.CourseName}");
                    }
                }
            }
            else
            {
                Console.WriteLine("преподаватель не найден");
            }
        }
    }

    private void ManageCourses()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("управление курсами");
            Console.WriteLine("1. добавить курс");
            Console.WriteLine("2. просмотреть все курсы");
            Console.WriteLine("3. просмотреть студентов на курсе");
            Console.WriteLine("4. назад");
            Console.Write("выберите опцию: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    AddCourse();
                    break;
                case "2":
                    ViewAllCourses();
                    break;
                case "3":
                    ViewCourseStudents();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("неверный выбор.");
                    break;
            }
            WaitForKey();
        }
    }

    private void AddCourse()
    {
        Console.Write("введите название курса: ");
        var name = Console.ReadLine();

        _universityManager.AddCourse(name);
        Console.WriteLine("курс успешно добавлен!");
    }

    private void ViewAllCourses()
    {
        var courses = _universityManager.GetAllCourses();
        if (courses.Count == 0)
        {
            Console.WriteLine("курсы не найдены.");
            return;
        }

        Console.WriteLine("\nсписок всех курсов:");
        foreach (var course in courses)
        {
            Console.WriteLine($"ID: {course.CourseId}, курс: {course.CourseName}");
            Console.WriteLine($"преподаватель: {(course.Teacher != null ? course.Teacher.Name : "не назначен")}");
            Console.WriteLine($"студентов: {course.EnrolledStudents.Count}");
            Console.WriteLine();
        }
    }

    private void ViewCourseStudents()
    {
        _universityManager.ShowAllCourses();
        Console.Write("\nвыберите ID курса для просмотра студентов: ");
        if (int.TryParse(Console.ReadLine(), out int courseId))
        {
            var info = _universityManager.GetCourseStudentsInfo(courseId);
            Console.WriteLine($"\nстуденты на курсе:");
            Console.WriteLine(info);
        }
        else
        {
            Console.WriteLine("некорректный ID.");
        }
    }

    private void ViewAllData()
    {
        Console.Clear();
        Console.WriteLine("данные университета\n");

        Console.WriteLine("студенты:");
        var students = _universityManager.GetAllStudents();
        if (students.Count == 0)
        {
            Console.WriteLine("студенты не найдены.");
        }
        else
        {
            foreach (var student in students)
            {
                Console.WriteLine(student.GetInfo());
            }
        }

        Console.WriteLine("\nпреподаватели:");
        var teachers = _universityManager.GetAllTeachers();
        if (teachers.Count == 0)
        {
            Console.WriteLine("преподаватели не найдены");
        }
        else
        {
            foreach (var teacher in teachers)
            {
                Console.WriteLine(teacher.GetInfo());
            }
        }

        Console.WriteLine("\nкурсы:");
        var courses = _universityManager.GetAllCourses();
        if (courses.Count == 0)
        {
            Console.WriteLine("курсы не найдены.");
        }
        else
        {
            foreach (var course in courses)
            {
                Console.WriteLine($"ID: {course.CourseId}, курс: {course.CourseName}");
                Console.WriteLine($"преподаватель: {(course.Teacher != null ? course.Teacher.Name : "не назначен")}");
                Console.WriteLine($"студентов: {course.EnrolledStudents.Count}");
                Console.WriteLine();
            }
        }

        WaitForKey();
    }

    private void WaitForKey()
    {
        Console.WriteLine("\nнажмите любую клавишу для продолжения");
        Console.ReadKey();
    }
}
public class Program
{
    public static void Main(string[] args)
    {
        var menu = new UniversityConsoleMenu();
        menu.Run();
    }
}