using RecordManagement.Core.Application.Validators;
using Xunit;

namespace RecordManagement.Tests;

public sealed class StudentValidatorTests {
    [Theory]
    [InlineData(" ")]
    [InlineData("This name is way too long and exceeds the maximum allowed length of one hundred characters so it"
                + " should fail validation")]
    public void Theory_WhenStudentNameIsInvalid_ThenThrowsException(string name) {
        // When & Then
        Assert.Throws<ArgumentException>(() => StudentValidator.ValidateName(name));
    }

    [Theory]
    [InlineData("J")]
    [InlineData("Yury")]
    [InlineData("LeBron James")]
    [InlineData("z")]
    [InlineData("AAA")]
    [InlineData("имя")]
    [InlineData("ИМЯ")]
    [InlineData("Very very very very very long name but still passes")]
    public void Theory_WhenStudentNameIsValid_ThenDoesNotThrowException(string name) {
        // When & Then
        StudentValidator.ValidateName(name);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Theory_WhenStudentGradeIsInvalid_thenThrowsException(int grade) {
        // When & Then
        Assert.Throws<ArgumentException>(() => StudentValidator.ValidateGrade(grade));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(47)]
    [InlineData(55)]
    public void Theory_WhenStudentGradeIsValid_ThenDoesNotThroesException(int grade) {
        // When & Then
        StudentValidator.ValidateGrade(grade);
    }

}
