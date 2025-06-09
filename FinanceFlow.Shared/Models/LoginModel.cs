using System.ComponentModel.DataAnnotations;

namespace FinanceFlow.Shared.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email bo� b�rak�lamaz.")]
        [EmailAddress]
        public string? Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "�ifre bo� b�rak�lamaz.")]
        public string Password { get; set; } =string.Empty;
    }
}