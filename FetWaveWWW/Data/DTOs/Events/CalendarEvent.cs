using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace FetWaveWWW.Data.DTOs.Events
{
    [Index(nameof(Unique_Id), IsUnique = true)]
    [Index(nameof(RegionId))]
    [Index(nameof(StartDate), nameof(EndDate))]
    public class CalendarEvent
    {
        //ID
        [Key]
        public int Id { get; set; }
        [Required]
        public Guid Unique_Id { get; set; } = Guid.NewGuid();

        //CRUD
        [Required]
        public DateTime CreatedTS { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedTS { get; set; }
        public DateTime? DeletedTS { get; set; }
        [Required]
        public string? CreatedUserId { get; set; }
        public string? UpdatedUserId { get; set; }

        //Meta-data
        public int? RegionId { get; set; }
        public int? CategoryId { get; set; }
        public int? DressCodeId { get; set; }

        //Event Info
        [Required]
        public DateTime? StartDate { get; set; }
        [Required]
        public DateTime? EndDate { get; set; }
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Address { get; set; }


        [ForeignKey(nameof(RegionId))]
        public virtual Region? Region { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public virtual Category? Category { get; set; }
        [ForeignKey(nameof(DressCodeId))]
        public virtual DressCode? DressCode { get; set; }

        [ForeignKey(nameof(CreatedUserId))]
        public virtual IdentityUser? CreatedUser { get; set; }
        [ForeignKey(nameof(UpdatedUserId))]
        public virtual IdentityUser? UpdatedUser { get; set; }

    }
}
