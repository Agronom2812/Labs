using System.Text.Json;
using RecordManagement.Core.Application.Services;
using RecordManagement.Core.Domain;

namespace RecordManagement.Core.DataAccess
{
    /// <summary>
    /// Provides data access operations for student records using JSON file storage.
    /// </summary>
    public sealed class StudentRepository : IStudentRepository
    {
        private const string FilePath = "students.json";
        private readonly List<Student> _students;

        /// <summary>
        /// Initializes repository and loads existing student data.
        /// </summary>
        public StudentRepository()
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                _students = JsonSerializer.Deserialize<List<Student>>(json) ?? [];
            }
            else
            {
                _students = [];
            }
        }

        /// <summary>
        /// Adds a new student to the repository.
        /// </summary>
        /// <param name="student">Student object to add.</param>
        public void AddStudent(Student? student) {
            if (student != null) _students.Add(student);
            SaveChanges();
        }

        /// <summary>
        /// Updates an existing student's information.
        /// </summary>
        /// <param name="student">Student object with updated data.</param>
        public void UpdateStudent(Student student)
        {
            var existingStudent = _students.Find(s => s.Id == student.Id);
            if (existingStudent == null) return;

            existingStudent.Name = student.Name;
            existingStudent.Grade = student.Grade;
            SaveChanges();
        }

        /// <summary>
        /// Deletes a student by ID.
        /// </summary>
        /// <param name="id">Student ID to delete.</param>
        public bool DeleteStudent(int id)
        {
            _students.RemoveAll(s => s.Id == id);
            SaveChanges();

            return true;
        }

        /// <summary>
        /// Retrieves all students from the repository.
        /// </summary>
        /// <returns>List of all students.</returns>
        public List<Student> GetAllStudents()
        {
            return _students;
        }

        /// <summary>
        /// Finds a student by their ID.
        /// </summary>
        /// <param name="id">Student ID to search for.</param>
        /// <returns>Student object if found, <c>null</c> otherwise.</returns>
        public Student? GetStudentById(int id)
        {
            return _students.Find(s => s.Id == id);
        }

        private void SaveChanges()
        {
            string json = JsonSerializer.Serialize(_students);
            File.WriteAllText(FilePath, json);
        }
    }
}
