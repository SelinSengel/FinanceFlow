using FinanceFlow.API.Data;
using FinanceFlow.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ArchivesController : ControllerBase
    {
        private readonly FinanceFlowDbContext _context;
        private readonly IArchiveService _archiveService;

        public ArchivesController(FinanceFlowDbContext context, IArchiveService archiveService)
        {
            _context = context;
            _archiveService = archiveService;
        }

        [HttpGet("months")]
        public IActionResult GetAvailableMonths()
        {
            var months = _context.ArchivedExpenses
                .Select(x => new { x.ArchivedYear, x.ArchivedMonth })
                .Distinct()
                .OrderByDescending(x => x.ArchivedYear)
                .ThenByDescending(x => x.ArchivedMonth)
                .ToList()
                .Select(m => $"{m.ArchivedYear}-{m.ArchivedMonth:D2}")
                .ToList();

            return Ok(months);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetArchivedExpenses(int userId, [FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                var query = _context.ArchivedExpenses
                    .Where(a => a.UserId == userId);

                if (year.HasValue)
                    query = query.Where(a => a.ArchivedYear == year.Value);

                if (month.HasValue)
                    query = query.Where(a => a.ArchivedMonth == month.Value);

                var result = await query
                    .OrderByDescending(x => x.ArchivedYear)
                    .ThenByDescending(x => x.ArchivedMonth)
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        [HttpPost("restore")]
        public async Task<IActionResult> Restore([FromQuery] int year, [FromQuery] int month)
        {
            await _archiveService.RestoreArchivedExpensesAsync(year, month);
            return Ok($"Restored archived expenses for {month:D2}/{year}");
        }
    }
}
