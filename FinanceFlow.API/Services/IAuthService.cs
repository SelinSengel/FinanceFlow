using FinanceFlow.Shared.Models;
public interface IAuthService
{
    Task<TokenResponseModel?> RegisterAsync(RegisterModel model);
    Task<TokenResponseModel?> RegisterCorporateAsync(CorporateRegisterModel model);
    Task<TokenResponseModel?> LoginAsync(LoginModel model);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
}