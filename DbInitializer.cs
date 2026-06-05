using EventEase.Models;

namespace EventEase.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Make sure the database 
            context.Database.EnsureCreated();

            // Check if there's already data
            if (context.Venues.Any() || context.Events.Any())
            {
                return; // Database has been seeded
            }

            // Venues with placeholder images
            var venues = new Venue[]
            {
                new Venue
                {
                    Name = "Grand Ballroom",
                    Location = "123 Main Street, Cape Town",
                    Capacity = 200,
                    ImageUrl = "https://picsum.photos/id/10/400/250"
                },
                new Venue
                {
                    Name = "Garden Terrace",
                    Location = "45 Beach Road, Durban",
                    Capacity = 75,
                    ImageUrl = "https://picsum.photos/id/15/400/250"
                },
                new Venue
                {
                    Name = "Executive Boardroom",
                    Location = "78 Business Park, Johannesburg",
                    Capacity = 25,
                    ImageUrl = "https://picsum.photos/id/20/400/250"
                },
                new Venue
                {
                    Name = "Rooftop Pavilion",
                    Location = "12 Skyline Drive, Pretoria",
                    Capacity = 150,
                    ImageUrl = "https://picsum.photos/id/30/400/250"
                }
            };
            context.Venues.AddRange(venues);

            // Events with placeholder images
            var events = new Event[]
            {
                new Event
                {
                    Name = "Annual Tech Conference",
                    Description = "A 2-day conference on latest tech trends",
                    StartDate = DateTime.Now.AddDays(30),
                    EndDate = DateTime.Now.AddDays(32),
                    ImageUrl = "https://picsum.photos/id/26/400/250"
                },
                new Event
                {
                    Name = "Wedding Expo",
                    Description = "Showcase of wedding vendors and services",
                    StartDate = DateTime.Now.AddDays(14),
                    EndDate = DateTime.Now.AddDays(14),
                    ImageUrl = "https://picsum.photos/id/29/400/250"
                },
                new Event
                {
                    Name = "Corporate Gala Dinner",
                    Description = "Annual company awards ceremony",
                    StartDate = DateTime.Now.AddDays(45),
                    EndDate = DateTime.Now.AddDays(45),
                    ImageUrl = "https://picsum.photos/id/36/400/250"
                },
                new Event
                {
                    Name = "Music Festival",
                    Description = "Live performances by local artists",
                    StartDate = DateTime.Now.AddDays(60),
                    EndDate = DateTime.Now.AddDays(62),
                    ImageUrl = "https://picsum.photos/id/40/400/250"
                }
            };
            context.Events.AddRange(events);

            context.SaveChanges();
        }
    }
}