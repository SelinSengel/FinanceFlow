using System.Text.Json.Serialization;

namespace FinanceFlow.Shared.Models {
public class ArchivedExpenseModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("comment")]
    public string Comment { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("archivedMonth")]
    public int ArchivedMonth { get; set; }

    [JsonPropertyName("archivedYear")]
    public int ArchivedYear { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}
 }