using BusinessLayer.Exceptions;
using System.Text.RegularExpressions;

namespace BusinessLayer.Rules
{
    public static partial class LabelRules
    {
        public static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Label name is required");

            // Trim whitespace
            name = name.Trim();

            if (name.Length < 1)
                throw new ValidationException("Label name must be at least 1 character long");

            if (name.Length > 50)
                throw new ValidationException("Label name cannot exceed 50 characters");

            // Check for valid characters (letters, numbers, spaces, hyphens, underscores)
            if (!IsValidLabelName(name))
                throw new ValidationException("Label name contains invalid characters. Only letters, numbers, spaces, hyphens, and underscores are allowed");

            // Prevent labels with only whitespace after trim
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Label name cannot be only whitespace");
        }

        public static void ValidateNameLength(string name, int minLength, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Label name is required");

            var trimmedName = name.Trim();

            if (trimmedName.Length < minLength)
                throw new ValidationException($"Label name must be at least {minLength} characters long");

            if (trimmedName.Length > maxLength)
                throw new ValidationException($"Label name cannot exceed {maxLength} characters");
        }

        private static bool IsValidLabelName(string name)
        {
            // Allow letters (any language), numbers, spaces, hyphens, and underscores
            var regex = LabelNameRegex();
            return regex.IsMatch(name);
        }

        [GeneratedRegex(@"^[\w\s\-]+$", RegexOptions.None)]
        private static partial Regex LabelNameRegex();
    }
}