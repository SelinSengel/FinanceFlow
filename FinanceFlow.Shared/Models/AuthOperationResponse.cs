namespace FinanceFlow.Shared.Models
{
    public class AuthOperationResponse
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
    }
}