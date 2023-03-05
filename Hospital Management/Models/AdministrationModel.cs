using System.ComponentModel.DataAnnotations;

namespace Hospital_Management.Models
{
    public class AdministrationModel
    {
        [Key]
        [Required]
        public int? Id { get; set; }

        [Required]
        public String? Name { get; set; }

        [Required]
        public string? Value { get; set; }
    }
}