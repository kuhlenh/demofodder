using Xunit;
using static System.Console;

namespace PersonDemo
{
    public class Program123
    {
        static void Main(string[] args)
        {
            Person p = new Person("Spike");
            WriteLine(PrintedForm(p));   
        }    

        public static string PrintedForm(Person p)
        {
            Student s;
            Teacher t;

            if ((s = p as Student) != null && s.Gpa > 3.5)
                return $"Honor Student {s.Name} ({s.Gpa})";
            else if (s != null)
                return $"Student {s.Name} ({s.Gpa})";
            else if ((t = p as Teacher) != null)
                return $"Teacher {t.Name} of {t.Subject}";
            else
                return $"Person {p.Name}";
        }
    }

    public class Person
    {
        public Person(string name) { this.Name = name; }
        public string Name { get; }
    }

    public class Student : Person
    {
        public Student(string name, double gpa) : base(name)
        {
            this.Gpa = gpa;
        }
        public double Gpa { get; }
    }

    public class Teacher : Person
    {
        public Teacher(string name, string subject) : base(name)
        {
            this.Subject = subject;
        }
        public string Subject { get; }
    }

    public class Test
    {
        Person[] persons = {
            new Student("Buffy", 3.5),
            new Student("Willow", 4.0),
            new Student("Xander", 2.0),
            new Teacher("Giles", "Librarian Studies"),
            new Person("Kasey")
        };

        [Fact]
        public void TestPFPerson()
        {
            Assert.Equal("Person Kasey", Program123.PrintedForm(persons[4]));
        }

        [Fact]
        public void TestPFStudent()
        {
            Assert.Equal("Honor Student Willow (4.0)", Program123.PrintedForm(persons[1]));
        }

        [Fact]
        public void TesetPFTeacher()
        {
            Assert.Equal("Teacher Giles of Librarian Studies", Program123.PrintedForm(persons[3]));
        }
    }
}




