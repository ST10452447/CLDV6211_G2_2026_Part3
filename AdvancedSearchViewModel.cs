using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EventEase.Models;

namespace EventEase.ViewModels
{
    public class AdvancedSearchViewModel
    {
        // Search Results
        public IEnumerable<Event> Events { get; set; } = new List<Event>();

        // Filter Criteria
        [Display(Name = "Event Type")]
        public int? EventTypeId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Only Show Available Venues")]
        public bool ShowOnlyAvailable { get; set; }

        // For Populating Dropdowns
        public IEnumerable<EventType> EventTypes { get; set; } = new List<EventType>();
    }
}