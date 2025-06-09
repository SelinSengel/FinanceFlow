namespace FinanceFlow.API.Entities
{
    public class PasswordResetTokenEntity
    {
        public int Id { get; set; }
        // Hangi kullanıcı için
        public int UserId { get; set; }
        public UserEntity User { get; set; } = null!;
        // Gerçek token değeri
        public string Token { get; set; } = null!;
        // Ne zamana kadar geçerli
        public DateTime ExpiresAt { get; set; }
    }
}
