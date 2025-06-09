using FinanceFlow.API.Entities;
using FinanceFlow.Shared.Models;

public interface IExpenseService
{
    Task<IEnumerable<ExpenseModel>> GetAllAsync(int userId);
    Task<IEnumerable<ExpenseModel>> GetAllSharedAsync();
    Task<ExpenseModel> CreateAsync(int userId, ExpenseModel m);
    Task<bool> UpdateAsync(int userId, ExpenseModel m);
    Task<bool> DeleteAsync(int userId, int expenseId);
    Task<IEnumerable<HighlightModel>> GetHighlightsAsync();
    Task<decimal> GetTodayTotalAsync();

}
