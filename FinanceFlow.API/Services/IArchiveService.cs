using System.Threading.Tasks;

namespace FinanceFlow.API.Services
{
    public interface IArchiveService
    {
        Task ArchiveIfNeededAsync();
        Task RestoreArchivedExpensesAsync(int year, int month);
    }
}
