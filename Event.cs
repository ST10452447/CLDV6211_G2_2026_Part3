using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Event name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Start Date & Time")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "End Date & Time")]
        public DateTime EndDate { get; set; }

        [Url]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        // File upload property (not saved to database)
        [NotMapped]
        [Display(Name = "Event Image")]
        public IFormFile? ImageFile { get; set; }

        // Navigation property for bookings
        public ICollection<Booking>? Bookings { get; set; }

        //  EventType relationship
        [Display(Name = "Event Type")]
        public int? EventTypeId { get; set; }

        [ForeignKey("EventTypeId")]
        public virtual EventType? EventType { get; set; }
    }
}