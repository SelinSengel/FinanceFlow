using System.ComponentModel.DataAnnotations;

namespace FinanceFlow.API.Entities
{
    public class CorporateProfileEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string TaxNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Sector { get; set; }

        public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
    }
}