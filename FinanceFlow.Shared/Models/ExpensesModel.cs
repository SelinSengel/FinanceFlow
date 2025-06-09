
namespace FinanceFlow.Shared.Models
{
    public class ExpenseModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        //public int? CorporateProfileId { get; set; } 
        public decimal Amount { get; set; }
        public string Category { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Comment { get; set; } = null!;
        public bool IsAnonymous { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsShared { get; set; }
        public string UserName { get; set; } = "Kullanıcı";

    }
}

