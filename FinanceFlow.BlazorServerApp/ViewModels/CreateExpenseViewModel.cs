// /ViewModels/CreateExpenseViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace FinanceFlow.BlazorServerApp.ViewModels
{
    public class CreateExpenseViewModel : IExpenseFormModel
    {
        [Required]
        public decimal Amount { get; set; }

        [Required, MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Comment { get; set; } = string.Empty;

        public bool IsAnonymous { get; set; }

        // Type modal formda seçilebilir veya sabitlenebilir
        public string Type { get; set; } = "Expense";
    }
}

// /ViewModels/UpdateExpenseViewModel.cs
namespace FinanceFlow.BlazorServerApp.ViewModels
{
    public class UpdateExpenseViewModel : CreateExpenseViewModel
    {
        public int Id { get; set; }
    }
}
