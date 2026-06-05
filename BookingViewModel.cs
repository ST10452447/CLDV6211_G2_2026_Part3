using System;
using EventEase.ViewModels; 

namespace EventEase.ViewModels
{
    /// <summary>
    /// ViewModel that combines data from Venue, Event, and Booking tables
    /// Used for the consolidated booking display with search functionality
    /// </summary>
    public class BookingViewModel
    {
        // Booking Information
        public int BookingId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }

        // Venue Information
        public string VenueName { get; set; } = string.Empty;
        public string VenueLocation { get; set; } = string.Empty;
        public int VenueCapacity { get; set; }
        public string? VenueImageUrl { get; set; }

        // Event Information
        public string EventName { get; set; } = string.Empty;
        public string? EventDescription { get; set; }
        public DateTime EventStartDate { get; set; }
        public DateTime EventEndDate { get; set; }
        public string? EventImageUrl { get; set; }

        // Computed Properties for display
        public string EventDateRange => $"{EventStartDate:dd MMM yyyy} - {EventEndDate:dd MMM yyyy}";
        public string BookingDateFormatted => BookingDate.ToString("dd MMM yyyy HH:mm");
        public bool IsUpcoming => EventStartDate > DateTime.Now;
        public bool IsOngoing => EventStartDate <= DateTime.Now && EventEndDate >= DateTime.Now;
        public string EventStatus => IsOngoing ? "🟢 Ongoing" : (IsUpcoming ? "🔵 Upcoming" : "⚫ Completed");
    }
}