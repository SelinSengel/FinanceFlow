using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.API.Entities
{
    [Index(nameof(Username), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class UserEntity
    {
        public int Id { get; set; }
        public string Role { get; set; } = "Individual";
        public int? CorporateProfileId { get; set; }
        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        [Required, MaxLength(200)]
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required, MaxLength(50)]
        public string FullName { get; set; } 
        public string? AvatarUrl { get; set; } 
        // Harcamalar için navigasyon:
        public ICollection<ExpenseEntity> Expenses { get; set; } = new List<ExpenseEntity>();
    }
}
