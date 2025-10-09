//ПР5


using System;
using System.Collections.Generic;
using System.Linq;

public abstract class Person
{
    private int Personid;
    private string Name;
    private int Age;
    private string Email;

    private Person(int personid, string name, int age, string email)
    {
        Personid = personid;
        Name = name;
        Age = age;
        Email = email;
    }
    public abstract string GetRole();

    public virtual string GetInfo()
    {
        return $"id: {Personid}, имя: {Name}, возраст: {Age}, email: {Email}";  
    }
}
public interface MyDisplay
{
    string DisplayInfo();
}
public class Student
{

}
