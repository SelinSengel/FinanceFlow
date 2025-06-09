using System.ComponentModel.DataAnnotations;

namespace FinanceFlow.Shared.Models
{
    public class TokenResponseModel 
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty ;
        public string Email { get; set; } = string.Empty;
    }
}