using FinanceFlow.API.Data;
using FinanceFlow.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.API.Services
{
    public class ArchiveService : IArchiveService
    {
        private readonly FinanceFlowDbContext _context;
        private readonly ILogger<ArchiveService> _logger;

        public ArchiveService(FinanceFlowDbContext context, ILogger<ArchiveService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ArchiveIfNeededAsync()
        {
            var now = DateTime.UtcNow;
            var archiveMonth = now.AddMonths(-1).Month;
            var archiveYear = now.AddMonths(-1).Year;

            bool alreadyArchived = await _context.ArchivedExpenses
                .AnyAsync(x => x.ArchivedMonth == archiveMonth && x.ArchivedYear == archiveYear);

            if (alreadyArchived)
                return;

            var entriesToArchive = await _context.Expenses
                .Where(e =>
                    e.CreatedAt.Month == archiveMonth &&
                    e.CreatedAt.Year == archiveYear &&
                    !e.IsShared &&
                    (e.Type == "Expense" || e.Type == "Income") // 🔥 SADECE bunlar arşivlenir
                )
                .ToListAsync();

            foreach (var e in entriesToArchive)
            {
                _context.ArchivedExpenses.Add(new ArchivedExpenseEntity
                {
                    UserId = e.UserId,
                    Amount = e.Amount,
                    Category = e.Category,
                    Comment = e.Comment,
                    CreatedAt = e.CreatedAt,
                    ArchivedMonth = archiveMonth,
                    ArchivedYear = archiveYear,
                    Type = e.Type // 🔄 Type bilgisini sakla
                });
            }

            _context.Expenses.RemoveRange(entriesToArchive);
            await _context.SaveChangesAsync();
        }

        public async Task RestoreArchivedExpensesAsync(int year, int month)
        {
            var archived = await _context.ArchivedExpenses
                .Where(a => a.ArchivedYear == year && a.ArchivedMonth == month)
                .ToListAsync();

            foreach (var a in archived)
            {
                // Type alanı yoksa varsayılanı 'Expense' veriyoruz
                _context.Expenses.Add(new ExpenseEntity
                {
                    UserId = a.UserId,
                    Amount = a.Amount,
                    Category = a.Category,
                    Comment = a.Comment,
                    CreatedAt = a.CreatedAt,
                    Type = a.Type ?? "Expense" // ⚠️ Type null ise fallback
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
