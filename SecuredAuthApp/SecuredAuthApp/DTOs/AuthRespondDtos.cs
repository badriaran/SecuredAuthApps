using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SecuredAuthApp.DTOs
{
    public class AuthRespondDtos
    {
        public string? Token { get; set; }=string.Empty;    
        public bool IsSuccess { get; set; } 
        public string? Message { get; set; }
    }
}
