

namespace ModelLayer.Configuration
{
    public class AppSettings
    {
        public string ApplicationName { get; set; } = string.Empty;
        public string ApplicationUrl { get; set; } = string.Empty;
        public int MaxLoginAttempts { get; set; }
        public int AccountLockoutMinutes { get; set; }
        public int OtpExpiryMinutes { get; set; }
        public int PasswordResetExpiryMinutes { get; set; }
        public int MaxNotesPerUser { get; set; }
        public int MaxLabelsPerUser { get; set; }
        public int MaxCollaboratorsPerNote { get; set; }
    }
}