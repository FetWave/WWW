using System.ComponentModel.DataAnnotations;

namespace FetWaveWWW.Data.DTOs.Events
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
    }
}
