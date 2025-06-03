namespace RecordManagement.Core.Application.Validators
{
    /// <summary>
    /// Provides validation methods for student data.
    /// </summary>
    public static class StudentValidator
    {
        /// <summary>
        /// Validates a student's name.
        /// </summary>
        /// <param name="name">Name to validate.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when name is empty or exceeds 100 characters.
        /// </exception>
        public static void ValidateName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty");

            if (name.Length > 100)
                throw new ArgumentException("Name is too long");
        }

        /// <summary>
        /// Validates a student's grade.
        /// </summary>
        /// <param name="grade">Grade to validate.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when grade is outside 0-100 range.
        /// </exception>
        public static void ValidateGrade(int grade)
        {
            if (grade is < 0 or > 100)
                throw new ArgumentException("Grade must be between 0 and 100");
        }
    }
}
