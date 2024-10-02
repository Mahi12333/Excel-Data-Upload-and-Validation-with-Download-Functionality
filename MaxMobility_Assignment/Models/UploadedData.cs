using System.ComponentModel.DataAnnotations;

namespace MaxMobility_Assignment.Models
{
    public class UploadedData
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MaxLength(10)]
        public required string Phone { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Address { get; set; }

        [Required]
        [MaxLength(10)]
        public required string Status { get; set; }  // "Success" or "Failed"
    }
}
