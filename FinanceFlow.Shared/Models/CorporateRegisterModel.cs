using System.ComponentModel.DataAnnotations;

namespace FinanceFlow.Shared.Models
{
    public class CorporateRegisterModel
    {
        [Required(ErrorMessage = "Kullanıcı adı boş bırakılamaz.")]
        [MaxLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre boş bırakılamaz.")]
        [MinLength(8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
        [RegularExpression(
               @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            ErrorMessage = "Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir."
        )]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Firma adı boş bırakılamaz.")]
        [MaxLength(100, ErrorMessage = "Firma adı en fazla 100 karakter olabilir.")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vergi dairesi boş bırakılamaz.")]
        [MaxLength(100, ErrorMessage = "Vergi dairesi en fazla 100 karakter olabilir.")]
        public string TaxOffice { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vergi numarası boş bırakılamaz.")]
        [MaxLength(20, ErrorMessage = "Vergi numarası en fazla 20 karakter olabilir.")]
        public string TaxNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kurumsal e-posta boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir kurumsal e-posta girin.")]
        [MaxLength(100, ErrorMessage = "Kurumsal e-posta en fazla 100 karakter olabilir.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sektör boş bırakılamaz.")]
        [MaxLength(50, ErrorMessage = "Sektör en fazla 50 karakter olabilir.")]
        public string? Sector { get; set; }
    }
}
