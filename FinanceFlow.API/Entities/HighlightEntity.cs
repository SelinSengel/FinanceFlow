using System.ComponentModel.DataAnnotations;

namespace FinanceFlow.API.Entities
{
    public class HighlightEntity
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        public int LikeCount { get; set; }
    }
}
