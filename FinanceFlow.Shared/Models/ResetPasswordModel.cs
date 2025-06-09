using System.ComponentModel.DataAnnotations;

namespace FinanceFlow.Shared.Models
{
    public class ResetPasswordModel
    {
        [Required]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "Yeni şifre boş bırakılamaz.")]
        [MinLength(8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }

}
