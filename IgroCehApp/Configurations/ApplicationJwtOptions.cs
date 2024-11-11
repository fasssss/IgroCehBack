using Microsoft.AspNetCore.Authentication;

namespace API.Configurations
{
    public class ApplicationJwtOptions: AuthenticationSchemeOptions
    {
        public string Secret { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public string ExpInHours { get; set; }
    }
}
