using FinanceFlow.API.Data;
using FinanceFlow.API.Services;
using FinanceFlow.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _svc;
        private readonly FinanceFlowDbContext _context;
        public ExpensesController(IExpenseService svc, FinanceFlowDbContext context)
        {
            _svc = svc;
            _context = context;
        }

        // GET api/expenses?category=...&type=...&dateFrom=...&dateTo=...&isAnonymous=...
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseModel>>> GetAll(
            [FromQuery] string? category,
            [FromQuery] string? type,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] bool? isAnonymous)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var items = await _svc.GetAllAsync(userId);
            var query = items.AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(type))
                query = query.Where(e => e.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
            if (dateFrom.HasValue)
                query = query.Where(e => e.CreatedAt >= dateFrom.Value);
            if (dateTo.HasValue)
                query = query.Where(e => e.CreatedAt <= dateTo.Value);
            if (isAnonymous.HasValue)
                query = query.Where(e => e.IsAnonymous == isAnonymous.Value);

            return Ok(query.ToList());
        }

        // GET api/expenses/income
        [HttpGet("income")]
        public async Task<ActionResult<IEnumerable<ExpenseModel>>> GetIncomes()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var incomes = (await _svc.GetAllAsync(userId))
                          .Where(e => e.Type.Equals("Income", StringComparison.OrdinalIgnoreCase));
            return Ok(incomes);
        }

        // GET api/expenses/expense
        [HttpGet("expense")]
        public async Task<ActionResult<IEnumerable<ExpenseModel>>> GetExpenses()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var expenses = (await _svc.GetAllAsync(userId))
                           .Where(e => e.Type.Equals("Expense", StringComparison.OrdinalIgnoreCase));
            return Ok(expenses);
        }

        // POST api/expenses/income
        [HttpPost("income")]
        public async Task<ActionResult<ExpenseModel>> AddIncome([FromBody] ExpenseModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            model.Type = "Income";
            var created = await _svc.CreateAsync(userId, model);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }

        // POST api/expenses/expense
        [HttpPost("expense")]
        public async Task<ActionResult<ExpenseModel>> AddExpense([FromBody] ExpenseModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            model.Type = "Expense";
            var created = await _svc.CreateAsync(userId, model);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }

        // PUT api/expenses/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ExpenseModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            model.Id = id;
            var ok = await _svc.UpdateAsync(userId, model);
            if (!ok) return NotFound();
            return NoContent();
        }

        // DELETE api/expenses/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ok = await _svc.DeleteAsync(userId, id);
            if (!ok) return NotFound();
            return NoContent();
        }
        [HttpGet("shared")]
        public async Task<ActionResult<IEnumerable<ExpenseModel>>> GetSharedExpenses()
        {
            var expenses = await _context.Expenses
                .Include(e => e.User)
                .Where(e => !string.IsNullOrWhiteSpace(e.Comment)) // yorum varsa
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new ExpenseModel
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    Amount = e.Amount,
                    Category = e.Category,
                    Type = e.Type,
                    Comment = e.Comment,
                    IsAnonymous = e.IsAnonymous,
                    CreatedAt = e.CreatedAt,
                    UserName = e.IsAnonymous ? "Anonim" : e.User.FullName
                })
                .ToListAsync();

            return expenses;
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAllExpenses()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);
            var expenses = await _svc.GetAllAsync(userId);
            return Ok(expenses);
        }

    }
}
