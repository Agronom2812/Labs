using System.Text.RegularExpressions;
using RecordManagement.Core.API;
using RecordManagement.Core.Application.Services;
using RecordManagement.Core.DataAccess;
using RecordManagement.Core.Domain;
using RecordManagement.Core.DTOs;

namespace RecordManagement
{
    internal static partial class Program
    {
        private static readonly StudentService s_studentService;

        static Program()
        {
            var httpClient = new HttpClient();
            var studentRepository = new StudentRepository();
            var quoteService = new QuoteService(httpClient);
            s_studentService = new StudentService(studentRepository, quoteService);
        }

        private static async Task Main()
        {
            while (true)
            {
                Console.Clear();
                ShowMainMenu();

                string option = GetCleanInput("Select an option: ");

                switch (option)
                {
                    case "1":
                        await AddStudentFlow();
                        break;
                    case "2":
                        ViewAllStudentsFlow();
                        break;
                    case "3":
                        DeleteStudentFlow();
                        break;
                    case "4":
                        return;
                    default:
                        ShowMessage("Invalid option. Please try again.", isError: true);
                        WaitForUser();
                        break;
                }
            }
        }

        private static void ShowMainMenu()
        {
            Console.WriteLine("=== Student Record Management System ===");
            Console.WriteLine("1. Add Student");
            Console.WriteLine("2. View All Students");
            Console.WriteLine("3. Delete Student");
            Console.WriteLine("4. Exit");
        }

        private static async Task AddStudentFlow()
        {
            Console.Clear();
            Console.WriteLine("=== Add New Student ===");
            Console.WriteLine("(Enter 'back' at any time to return to main menu)\n");

            string name;
            while (true)
            {
                name = GetCleanInput("Enter student name: ");
                if (IsBackCommand(name)) return;

                if (string.IsNullOrWhiteSpace(name))
                {
                    ShowMessage("Name cannot be empty!", isError: true);
                    continue;
                }

                if (!IsValidName(name))
                {
                    ShowMessage("Name can only contain letters and must have at least one letter!", isError: true);
                    continue;
                }

                break;
            }

            int grade;
            while (true)
            {
                string gradeInput = GetCleanInput("Enter student grade (0-100): ");
                if (IsBackCommand(gradeInput)) return;

                if (!int.TryParse(gradeInput, out grade) || grade < 0 || grade > 100)
                {
                    ShowMessage("Grade must be a number between 0 and 100!", isError: true);
                    continue;
                }
                break;
            }

            try
            {
                var studentDto = new StudentDTO { Name = name.Trim(), Grade = grade };
                await s_studentService.AddStudentAsync(studentDto);

                ShowMessage("\nStudent added successfully!", isError: false);
            }
            catch (Exception ex)
            {
                ShowMessage($"\nError: {ex.Message}", isError: true);
            }

            WaitForUser();
        }

        private static void ViewAllStudentsFlow()
        {
            Console.Clear();
            Console.WriteLine("=== List of Students ===\n");

            var students = s_studentService.GetAllStudents();
            if (students.Count == 0)
            {
                Console.WriteLine("No students found.");
            }
            else
            {
                foreach (var student in students)
                {
                    Console.WriteLine($"ID: {student.Id}");
                    Console.WriteLine($"Name: {student.Name}");
                    Console.WriteLine($"Grade: {student.Grade}");

                    Console.WriteLine("-------------------");
                }
            }

            WaitForUser();
        }

        private static void DeleteStudentFlow()
        {
            Console.Clear();
            Console.WriteLine("=== Delete Student ===");
            Console.WriteLine("(Enter 'back' at any time to return to main menu)\n");

            var students = s_studentService.GetAllStudents();
            if (students.Count == 0)
            {
                ShowMessage("No students available to delete.", isError: false);
                WaitForUser();
                return;
            }

            Console.WriteLine("Available students:\n");
            foreach (var student in students.OfType<Student>()) {
                Console.WriteLine($"ID: {student.Id}, Name: {student.Name}");
            }

            int studentId;
            while (true)
            {
                string idInput = GetCleanInput("\nEnter student ID to delete: ");
                if (IsBackCommand(idInput)) return;

                if (!int.TryParse(idInput, out studentId))
                {
                    ShowMessage("Invalid ID format! Please enter a number.", isError: true);
                    continue;
                }
                break;
            }

            try
            {
                bool isDeleted = s_studentService.DeleteStudent(studentId);
                if (isDeleted)
                {
                    ShowMessage("\nStudent deleted successfully!", isError: false);
                }
                else
                {
                    ShowMessage("\nStudent not found with the given ID.", isError: true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"\nError: {ex.Message}", isError: true);
            }

            WaitForUser();
        }

        private static string GetCleanInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }

        private static bool IsBackCommand(string input)
        {
            return input.Equals("back", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsValidName(string name)
        {
            return MyRegex().IsMatch(name) && MyRegex1().IsMatch(name);
        }

        private static void ShowMessage(string message, bool isError)
        {
            Console.ForegroundColor = isError ? ConsoleColor.Red : ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void WaitForUser()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        [GeneratedRegex(@"^[a-zA-Zа-яА-ЯёЁ\s]+$")]
        private static partial Regex MyRegex();
        [GeneratedRegex(@"[a-zA-Zа-яА-ЯёЁ]")]
        private static partial Regex MyRegex1();
    }
}
