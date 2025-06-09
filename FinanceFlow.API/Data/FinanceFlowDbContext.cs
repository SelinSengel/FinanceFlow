using Microsoft.EntityFrameworkCore;
using FinanceFlow.API.Entities;

namespace FinanceFlow.API.Data
{
    public class FinanceFlowDbContext : DbContext
    {
        public FinanceFlowDbContext(DbContextOptions<FinanceFlowDbContext> options) : base(options)
        {
        }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<ExpenseEntity> Expenses { get; set; }
        public DbSet<CorporateProfileEntity> CorporateProfiles { get; set; }
        public DbSet<PasswordResetTokenEntity> PasswordResetTokens { get; set; } = null!;
        public DbSet<HighlightEntity> Highlights { get; set; }
        public DbSet<ArchivedExpenseEntity> ArchivedExpenses { get; set; }
    }
}