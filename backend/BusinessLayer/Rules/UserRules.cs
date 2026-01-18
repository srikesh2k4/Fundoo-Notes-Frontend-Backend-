using BusinessLayer.Exceptions;
using ModelLayer.DTOs.Auth;
using System.Text.RegularExpressions;

namespace BusinessLayer.Rules
{
    public static partial class UserRules
    {
        public static void ValidateRegistration(RegisterRequestDto dto)
        {
            // Name validation
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Name is required");

            if (dto.Name.Length < 2)
                throw new ValidationException("Name must be at least 2 characters long");

            if (dto.Name.Length > 100)
                throw new ValidationException("Name cannot exceed 100 characters");

            // Only allow letters, spaces, and basic punctuation
            if (!IsValidName(dto.Name))
                throw new ValidationException("Name contains invalid characters");

            // Email validation
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ValidationException("Email is required");

            if (!IsValidEmail(dto.Email))
                throw new ValidationException("Invalid email format");

            if (dto.Email.Length > 255)
                throw new ValidationException("Email cannot exceed 255 characters");

            // Password validation
            ValidatePassword(dto.Password);
        }

        public static void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ValidationException("Password is required");

            if (password.Length < 6)
                throw new ValidationException("Password must be at least 6 characters long");

            if (password.Length > 100)
                throw new ValidationException("Password cannot exceed 100 characters");

            // Check for at least one uppercase letter
            if (!password.Any(char.IsUpper))
                throw new ValidationException("Password must contain at least one uppercase letter");

            // Check for at least one lowercase letter
            if (!password.Any(char.IsLower))
                throw new ValidationException("Password must contain at least one lowercase letter");

            // Check for at least one digit
            if (!password.Any(char.IsDigit))
                throw new ValidationException("Password must contain at least one number");
        }

        public static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ValidationException("Email is required");

            if (!IsValidEmail(email))
                throw new ValidationException("Invalid email format");
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = EmailRegex();
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidName(string name)
        {
            // Allow letters, spaces, hyphens, apostrophes, and periods
            var regex = NameRegex();
            return regex.IsMatch(name);
        }

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
        private static partial Regex EmailRegex();

        [GeneratedRegex(@"^[a-zA-Z\s\-'.]+$")]
        private static partial Regex NameRegex();
    }
}