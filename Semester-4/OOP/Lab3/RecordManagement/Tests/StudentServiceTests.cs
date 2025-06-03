using RecordManagement.Core.API;
using RecordManagement.Core.Application.Services;
using RecordManagement.Core.DataAccess;
using RecordManagement.Core.Domain;
using RecordManagement.Core.DTOs;
using Moq;
using Xunit;

namespace RecordManagement.Tests
{
    public sealed class StudentServiceTests
    {
        private readonly Mock<IStudentRepository> _mockRepo;
        private readonly Mock<IQuoteService> _mockQuoteService;
        private readonly StudentService _service;

        public StudentServiceTests()
        {
            _mockRepo = new Mock<IStudentRepository>();
            _mockQuoteService = new Mock<IQuoteService>();
            _service = new StudentService(_mockRepo.Object, _mockQuoteService.Object);
        }

        [Fact]
        public async Task AddStudentAsync_ValidInput_AddsStudentAndShowsQuote()
        {
            // Given
            var testDto = new StudentDTO { Name = "John Doe", Grade = 85 };
            var testQuote = new QuoteDTO { Content = "Test quote", Author = "Test Author" };

            _mockRepo.Setup(x => x.GetAllStudents())
                .Returns([]);

            _mockQuoteService.Setup(x => x.GetRandomQuoteAsync())
                .ReturnsAsync(testQuote);

            Student? addedStudent = null;
            _mockRepo.Setup(x => x.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => addedStudent = s);

            // When
            await _service.AddStudentAsync(testDto);

            // Then
            _mockRepo.Verify(x => x.AddStudent(It.IsAny<Student>()), Times.Once);
            Assert.NotNull(addedStudent);
            Assert.Equal(testDto.Name, addedStudent.Name);
            Assert.Equal(testDto.Grade, addedStudent.Grade);
            _mockQuoteService.Verify(x => x.GetRandomQuoteAsync(), Times.Once);

            Assert.Equal(1, addedStudent.Id);
        }

        [Theory]
        [InlineData("", 50)]
        [InlineData("John", -1)]
        [InlineData("John", 101)]
        [InlineData(null, 50)]
        public async Task AddStudentAsync_InvalidInput_ThrowsException(string? name, int grade)
        {
            // Given
            var testDto = new StudentDTO { Name = name, Grade = grade };

            // When & Then
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddStudentAsync(testDto));
        }

        [Fact]
        public void DeleteStudent_ExistingId_DeletesStudent()
        {
            // Given
            var testStudent = new Student { Id = 10, Name = "Test", Grade = 50 };
            _mockRepo.Setup(x => x.GetStudentById(1)).Returns(testStudent);

            // When
            bool result = _service.DeleteStudent(10);

            // Then
            Assert.True(result);
            _mockRepo.Verify(x => x.DeleteStudent(10), Times.Once);
        }

        [Fact]
        public void GetAllStudents_ReturnsAllStudents()
        {
            // Given
            var testStudents = new List<Student>
            {
                new() { Id = 1, Name = "Test1", Grade = 50 },
                new() { Id = 2, Name = "Test2", Grade = 60 }
            };
            _mockRepo.Setup(x => x.GetAllStudents()).Returns(testStudents);

            // When
            var result = _service.GetAllStudents();

            // Then
            Assert.Equal(2, result.Count);
            Assert.Equal(testStudents, result);
        }

        [Fact]
        public void GetStudentById_ExistingId_ReturnsStudent()
        {
            // Given
            var testStudent = new Student { Id = 1, Name = "Test", Grade = 50 };
            _mockRepo.Setup(x => x.GetStudentById(1)).Returns(testStudent);

            // When
            var result = _service.GetStudentById(1);

            // Then
            Assert.NotNull(result);
            Assert.Equal(testStudent, result);
        }

        [Fact]
        public void GetStudentById_NonExistingId_ReturnsNull()
        {
            // Given
            _mockRepo.Setup(x => x.GetStudentById(It.IsAny<int>())).Returns((Student)null);

            // When
            var result = _service.GetStudentById(999);

            // Then
            Assert.Null(result);
        }
    }
}
