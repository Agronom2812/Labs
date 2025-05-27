using RecordManagement.Core.API;
using RecordManagement.Core.Application.Validators;
using RecordManagement.Core.DataAccess;
using RecordManagement.Core.Domain;
using RecordManagement.Core.DTOs;

namespace RecordManagement.Core.Application.Services
{
    /// <summary>
    /// Provides student management services including CRUD operations.
    /// </summary>
    /// <param name="studentRepository">The repository for student data access.</param>
    /// <param name="quoteService">The service for retrieving motivational quotes.</param>
    public sealed class StudentService(IStudentRepository studentRepository, IQuoteService quoteService)
    {
        /// <summary>
        /// Adds a new student to the system.
        /// </summary>
        /// <param name="studentDto">Student data transfer object.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when name or grade validation fails.
        /// </exception>
        /// <remarks>
        /// Displays a motivational quote after successful addition.
        /// </remarks>
        public async Task AddStudentAsync(StudentDTO studentDto)
        {
            StudentValidator.ValidateName(studentDto.Name);
            StudentValidator.ValidateGrade(studentDto.Grade);

            var student = new Student
            {
                Id = GenerateId(),
                Name = studentDto.Name,
                Grade = studentDto.Grade
            };

            studentRepository.AddStudent(student);

            var quote = await quoteService.GetRandomQuoteAsync();
            if (quote == null) return;

            Console.WriteLine($"\n=== Motivational Quote ===");
            Console.WriteLine($"\"{quote.Content}\" - {quote.Author}\n");
        }

        /// <summary>
        /// Deletes a student by ID.
        /// </summary>
        /// <param name="id">Student ID to delete.</param>
        /// <returns><c>true</c> if deletion was successful.</returns>
        public bool DeleteStudent(int id)
        {
            studentRepository.GetStudentById(id);

            studentRepository.DeleteStudent(id);
            return true;

        }

        /// <summary>
        /// Updates an existing student's information.
        /// </summary>
        /// <param name="student">Student object with updated data.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when name or grade validation fails.
        /// </exception>
        public void UpdateStudent(Student student)
        {
            StudentValidator.ValidateName(student.Name);
            StudentValidator.ValidateGrade(student.Grade);
            studentRepository.UpdateStudent(student);
        }

        /// <summary>
        /// Retrieves all students in the system.
        /// </summary>
        /// <returns>List of all students.</returns>
        public List<Student> GetAllStudents()
        {
            return studentRepository.GetAllStudents();
        }

        /// <summary>
        /// Checks if student with given id exists.
        /// </summary>
        /// <param name="id">Student's ID to check.</param>
        /// <returns>
        /// <c>true</c> if student exists.
        /// <c>false</c> if student doesn't exist.
        /// </returns>
        public Student? GetStudentById(int id)
        {
            return studentRepository.GetStudentById(id);
        }

        private int GenerateId()
        {
            var students = studentRepository.GetAllStudents();
            return students.Count > 0 ? students[^1].Id + 1 : 1;
        }
    }
}
