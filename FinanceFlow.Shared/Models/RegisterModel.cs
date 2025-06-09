using System.ComponentModel.DataAnnotations;

namespace FinanceFlow.Shared.Models
{
    public class RegisterModel
    {
        public string Role { get; set; } = "Individual";

        [Required(ErrorMessage = "Kullanıcı adı boş bırakılamaz.")]
        [MinLength(3, ErrorMessage = "Kullanıcı adı en az 3 karakter olmalıdır.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "E-posta boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Şifre boş bırakılamaz.")]
        [MinLength(8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
        [RegularExpression(
               @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            ErrorMessage = "Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir."
        )]
        public string? Password { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
