
namespace ModelLayer.Configuration
{
    public class SecuritySettings
    {
        public bool RequireHttpsMetadata { get; set; }
        public bool SaveToken { get; set; }
        public bool ValidateIssuer { get; set; }
        public bool ValidateAudience { get; set; }
        public bool ValidateLifetime { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }
        public int ClockSkew { get; set; }
    }
}