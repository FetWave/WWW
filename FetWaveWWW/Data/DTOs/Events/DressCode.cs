using NuGet.Protocol;
using System.ComponentModel.DataAnnotations;

namespace FetWaveWWW.Data.DTOs.Events
{
    public class DressCode
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Details { get; set; }
    }
}
