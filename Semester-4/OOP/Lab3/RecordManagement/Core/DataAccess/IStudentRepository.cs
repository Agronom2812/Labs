using RecordManagement.Core.Domain;

namespace RecordManagement.Core.DataAccess
{
    public interface IStudentRepository
    {
        void AddStudent(Student? student);
        void UpdateStudent(Student student);
        bool DeleteStudent(int id);
        List<Student> GetAllStudents();
        Student? GetStudentById(int id);
    }
}
