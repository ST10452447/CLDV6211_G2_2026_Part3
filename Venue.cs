using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;  // For [NotMapped]

namespace EventEase.Models
{
    public class Venue
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Venue name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int Capacity { get; set; }

        [Url]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }  // Stores the blob URL after upload

        // ★ NEW: For file upload from the form
        // [NotMapped] means this property is NOT saved to the database
        [NotMapped]
        [Display(Name = "Venue Image")]
        public IFormFile? ImageFile { get; set; }

        // Navigation property - links to bookings
        public ICollection<Booking>? Bookings { get; set; }
    }
}