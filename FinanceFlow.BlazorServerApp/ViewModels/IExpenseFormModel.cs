namespace FinanceFlow.BlazorServerApp.ViewModels
{
    public interface IExpenseFormModel
    {
        decimal Amount { get; set; }
        string Category { get; set; }
        string Comment { get; set; }
        bool IsAnonymous { get; set; }
        string Type { get; set; }
    }
}