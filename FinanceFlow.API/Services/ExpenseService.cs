using Microsoft.EntityFrameworkCore;
using FinanceFlow.API.Data;
using FinanceFlow.API.Entities;
using FinanceFlow.Shared.Models;

namespace FinanceFlow.API.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly FinanceFlowDbContext _db;
        public ExpenseService(FinanceFlowDbContext db) => _db = db;

        // Kullanıcının kendi harcamaları
        public async Task<IEnumerable<ExpenseModel>> GetAllAsync(int userId)
        {
            var entities = await _db.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return entities.Select(e => MapToModel(e));
        }

        // Paylaşılan (anonim olmayan) tüm harcamalar
        public async Task<IEnumerable<ExpenseModel>> GetAllSharedAsync()
        {
            var entities = await _db.Expenses
                .Where(e => !e.IsAnonymous)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return entities.Select(e => MapToModel(e));
        }

        // Tek bir harcamayı ID ile getir (isteğe bağlı)
        public async Task<ExpenseModel?> GetByIdAsync(int userId, int id)
        {
            var e = await _db.Expenses
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            return e is null ? null : MapToModel(e);
        }

        // Yeni harcama ekle
        public async Task<ExpenseModel> CreateAsync(int userId, ExpenseModel m)
        {
            var entity = new ExpenseEntity
            {
                UserId = userId,
                Amount = m.Amount,
                Category = m.Category,
                Type = m.Type,
                Comment = m.Comment,
                IsAnonymous = m.IsAnonymous,
                IsShared = m.IsShared,
                CreatedAt = DateTime.UtcNow
            };

            _db.Expenses.Add(entity);
            await _db.SaveChangesAsync();

            return MapToModel(entity);
        }

        // Mevcut harcamayı güncelle
        public async Task<bool> UpdateAsync(int userId, ExpenseModel m)
        {
            var entity = await _db.Expenses
                .FirstOrDefaultAsync(x => x.Id == m.Id && x.UserId == userId);
            if (entity == null) return false;

            entity.Amount = m.Amount;
            entity.Category = m.Category;
            entity.Type = m.Type;
            entity.Comment = m.Comment;
            entity.IsAnonymous = m.IsAnonymous;
            // CreatedAt'ı genelde değiştirmiyoruz

            await _db.SaveChangesAsync();
            return true;
        }

        // Harcamayı sil
        public async Task<bool> DeleteAsync(int userId, int id)
        {
            var entity = await _db.Expenses
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (entity == null) return false;

            _db.Expenses.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        // Öne çıkan yerler
        public async Task<IEnumerable<HighlightModel>> GetHighlightsAsync()
        {
            var list = await _db.Highlights
                .OrderByDescending(h => h.LikeCount)
                .Take(5)
                .ToListAsync();

            return list.Select(h => new HighlightModel
            {
                Id = h.Id,
                Name = h.Name,
                LikeCount = h.LikeCount
            });
        }

        // Bugünün toplam harcaması
        public async Task<decimal> GetTodayTotalAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _db.Expenses
                .Where(e => e.CreatedAt >= today && e.CreatedAt < today.AddDays(1))
                .SumAsync(e => e.Amount);
        }

        // Helper: ExpenseEntity → ExpenseModel
        private static ExpenseModel MapToModel(ExpenseEntity e) => new()
        {
            Id = e.Id,
            UserId = e.UserId,
            Amount = e.Amount,
            Category = e.Category,
            Type = e.Type,
            Comment = e.Comment,
            IsAnonymous = e.IsAnonymous,
            IsShared = e.IsShared,
            CreatedAt = e.CreatedAt
        };


    }
}
