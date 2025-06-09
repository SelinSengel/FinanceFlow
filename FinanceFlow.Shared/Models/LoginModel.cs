using System.ComponentModel.DataAnnotations;

namespace FinanceFlow.Shared.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email boþ býrakýlamaz.")]
        [EmailAddress]
        public string? Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Þifre boþ býrakýlamaz.")]
        public string Password { get; set; } =string.Empty;
    }
}