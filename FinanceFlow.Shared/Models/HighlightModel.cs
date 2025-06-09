
namespace FinanceFlow.Shared.Models
{
    public class HighlightModel
    {
        public int Id { get; set; }

        // Örneğin: Kuaför, Market, Restoran vs.
        public string Name { get; set; } = string.Empty;

        // Eğer varsa ikon ya da resim URL’si
        public string? IconUrl { get; set; }

        // Popülerlik metriği (like sayısı vb.)
        public int LikeCount { get; set; }
    }
}
