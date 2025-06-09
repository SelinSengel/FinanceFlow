using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceFlow.API.Entities
{
    public class ExpenseEntity
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        //public int? CorporateProfileId { get; set; } 

        [ForeignKey(nameof(UserId))]
        public UserEntity? User { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Örn: Market, Fatura
        [Required, MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        // "Income" veya "Expense"
        [Required, MaxLength(50)]
        public string Type { get; set; } = "Expense";
        
        [Required, MaxLength(500)]
        public string Comment { get; set; } = string.Empty;    
        public bool IsAnonymous { get; set; }
        public bool IsShared { get; set; } = false; // kişisel mi, toplulukta mı
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
