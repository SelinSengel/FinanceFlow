using System.ComponentModel.DataAnnotations;

namespace FinanceFlow.Shared.Models
{
public class ForgotPasswordModel
{
    [Required(ErrorMessage = "E-posta boş bırakılamaz.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
    public string Email { get; set; } = null!;
}
}