

namespace FinanceFlow.API.Entities
{
    public class ArchivedExpenseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public decimal Amount { get; set; }
        public string Category { get; set; }
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }
        public int ArchivedMonth { get; set; }
        public int ArchivedYear { get; set; }
        public string Type { get; set; }
    }
}
