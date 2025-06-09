using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AIController : ControllerBase
{
    private readonly OpenAIService _aiService;
    private readonly IExpenseService _expenseService;

    public AIController(OpenAIService aiService, IExpenseService expenseService)
    {
        _aiService = aiService;
        _expenseService = expenseService;
    }

    [HttpPost("income")]
    public async Task<IActionResult> GetIncomeInsight()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized("Kullanıcı kimliği bulunamadı.");

        int userId = int.Parse(userIdClaim);
        var all = await _expenseService.GetAllAsync(userId);
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var incomes = all.Where(e => e.Type.Equals("income", StringComparison.OrdinalIgnoreCase) && e.CreatedAt >= startOfMonth).ToList();

        if (!incomes.Any())
            return Ok("Bu ay için gelir verisi bulunamadı.");

        var summary = string.Join(", ", incomes.Select(i => $"{i.Category} {i.Amount:N0} TL"));
        var prompt = $"Sen kullanıcı verilerini analiz eden bir finansal danışmansın. " +
                     $"Bu ayın gelir kayıtları: {summary}. " +
                     $"Gelir çeşitliliğini, düzenliliğini ve sürdürülebilirliğini değerlendir. " +
                     $"Önceki aylarla kıyaslandığında dikkat çeken bir eğilim ya da risk var mı? " +
                     $"Kısa, net ve insansı öneriler sun.";
        var result = await _aiService.AskAsync(prompt);
        return Ok(result);
    }

    [HttpPost("expense")]
    public async Task<IActionResult> GetExpenseInsight()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized("Kullanıcı kimliği bulunamadı.");

        int userId = int.Parse(userIdClaim);
        var all = await _expenseService.GetAllAsync(userId);
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var expenses = all.Where(e => e.Type.Equals("expense", StringComparison.OrdinalIgnoreCase) && e.CreatedAt >= startOfMonth).ToList();

        if (!expenses.Any())
            return Ok("Bu ay için gider verisi bulunamadı.");

        var summary = string.Join(", ", expenses.Select(i => $"{i.Category} {i.Amount:N0} TL"));
        var prompt = $"Sen kullanıcı verilerini analiz eden bir finansal danışmansın. " +
                     $"Bu ayın gider kayıtları: {summary}. " +
                     $"Giderleri kategori bazında değerlendirerek, özellikle tasarruf edilebilecek alanlara dair önerilerini paylaş. " +
                     $"Önceki aylarla karşılaştırıldığında bu ayın harcamalarında dikkat çeken eğilimler neler? " +
                     $"Lütfen kısa, net ve insansı cevaplar ver.";

        var result = await _aiService.AskAsync(prompt);
        return Ok(result);
    }

    [HttpPost("balance")]
    public async Task<IActionResult> GetBalanceInsight()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized("Kullanıcı kimliği bulunamadı.");

        int userId = int.Parse(userIdClaim);
        var all = await _expenseService.GetAllAsync(userId);
        var totalIncome = all.Where(x => x.Type.Equals("income", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Amount);
        var totalExpense = all.Where(x => x.Type.Equals("expense", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Amount);

        var prompt = $"Sen kullanıcı verilerini analiz eden finansal bir danışmansın. Verilere dayalı yorumlar yaparak kısa, net ve insansı analizler sun." +
                     $"Ayrıca, bekleyen işlem trendleri önceki aylarla karşılaştırıldığında nasıl bir değişim gösteriyor? Artış veya azalma var mı? " +
                     $"Lütfen genel bütçe durumu ve bekleyen harcamaların trendiyle ilgili değerlendirme yap ve bazı öneriler sun.";

        var result = await _aiService.AskAsync(prompt);
        return Ok(result);
    }

    [HttpPost("chat")]
    public async Task<IActionResult> ChatWithAI([FromBody] string userMessage)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized("Kullanıcı kimliği bulunamadı.");

        int userId = int.Parse(userIdClaim);
        var all = await _expenseService.GetAllAsync(userId);
        var totalIncome = all.Where(x => x.Type.Equals("income", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Amount);
        var totalExpense = all.Where(x => x.Type.Equals("expense", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Amount);

        var systemMessage = $"""
Sen kullanıcıya finansal konularda öneriler sunan, samimi, çok yönlü ve pratik bir yapay zekâ asistanısın.

• Kullanıcı gelir, gider, tasarruf, yatırım veya bütçe ile ilgili bir soru sorduğunda kısa, net ve kullanıcı dostu açıklamalar yap.
• Kullanıcının genel gelir-gider durumuna uygun önerilerde bulun: birikim planları, tasarruf yöntemleri veya harcama kontrolü gibi.
• Kullanıcı yatırım yapmak istiyorum, borsaya girmek istiyorum, şu an trend olan yatırım alanı nedir gibi sorular sorarsa:
  - Güncel piyasa trendlerini genel hatlarıyla açıklayıp,
  - Riskleri ve dikkat edilmesi gerekenleri belirt,
  - Kullanıcıyı bilinçli karar vermeye yönlendir.
• Kesin kazanç vaatlerinde bulunma, her zaman riskleri hatırlat.
• Selamlaşmalara sıcak ve kısa bir şekilde karşılık ver.
• Yanıtların 50-100 kelime aralığında, sade, motive edici ve insansı olmalı.
• Gerektiğinde kullanıcıyı destekleyici, motive edici ifadeler kullan ("Güzel bir hedef!", "Yavaş ve istikrarlı ilerlemek en doğrusu." gibi).
""";

        var result = await _aiService.AskAsync(userMessage, systemMessage);
        return Ok(result);
    }
}
