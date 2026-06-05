using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventEase.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings with Search and Sort
        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = searchString;

            ViewData["IdSortParm"] = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["EventSortParm"] = sortOrder == "event_asc" ? "event_desc" : "event_asc";
            ViewData["DateSortParm"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";

            // ★ FIXED: Query using JOIN instead of Select
            var query = from b in _context.Bookings
                        join v in _context.Venues on b.VenueId equals v.Id
                        join e in _context.Events on b.EventId equals e.Id
                        select new BookingViewModel
                        {
                            BookingId = b.Id,
                            CustomerName = b.CustomerName,
                            CustomerEmail = b.CustomerEmail,
                            BookingDate = b.BookingDate,
                            VenueName = v.Name,
                            VenueLocation = v.Location,
                            VenueCapacity = v.Capacity,
                            VenueImageUrl = v.ImageUrl,
                            EventName = e.Name,
                            EventDescription = e.Description,
                            EventStartDate = e.StartDate,
                            EventEndDate = e.EndDate,
                            EventImageUrl = e.ImageUrl
                        };

            var bookings = query.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b =>
                    b.BookingId.ToString().Contains(searchString) ||
                    b.EventName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    b.CustomerName.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting
            sortOrder = string.IsNullOrEmpty(sortOrder) ? "id_asc" : sortOrder;

            bookings = sortOrder switch
            {
                "id_desc" => bookings.OrderByDescending(b => b.BookingId),
                "event_asc" => bookings.OrderBy(b => b.EventName),
                "event_desc" => bookings.OrderByDescending(b => b.EventName),
                "date_asc" => bookings.OrderBy(b => b.EventStartDate),
                "date_desc" => bookings.OrderByDescending(b => b.EventStartDate),
                _ => bookings.OrderBy(b => b.BookingId)
            };

            return View(await bookings.ToListAsync());
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name");
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Name");
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VenueId,EventId,CustomerName,CustomerEmail")] Booking booking)
        {
            ModelState.Remove("Venue");
            ModelState.Remove("Event");

            if (ModelState.IsValid)
            {
                var selectedEvent = await _context.Events.FindAsync(booking.EventId);

                if (selectedEvent == null)
                {
                    TempData["ErrorMessage"] = "Selected event not found.";
                    PopulateDropdowns(booking);
                    return View(booking);
                }

                // Double booking check
                var conflictingBooking = await _context.Bookings
                    .Include(b => b.Event)
                    .Where(b => b.VenueId == booking.VenueId)
                    .Where(b => b.Event.StartDate < selectedEvent.EndDate
                            && b.Event.EndDate > selectedEvent.StartDate)
                    .FirstOrDefaultAsync();

                if (conflictingBooking != null)
                {
                    TempData["ErrorMessage"] = $"This venue is already booked from {conflictingBooking.Event.StartDate:dd MMM yyyy} to {conflictingBooking.Event.EndDate:dd MMM yyyy}.";
                    PopulateDropdowns(booking);
                    return View(booking);
                }

                booking.BookingDate = DateTime.Now;
                _context.Add(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Booking created for {booking.CustomerName}!";
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(booking);
            return View(booking);
        }

        private void PopulateDropdowns(Booking booking)
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name", booking.VenueId);
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Name", booking.EventId);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name", booking.VenueId);
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Name", booking.EventId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VenueId,EventId,CustomerName,CustomerEmail,BookingDate")] Booking booking)
        {
            if (id != booking.Id) return NotFound();

            ModelState.Remove("Venue");
            ModelState.Remove("Event");

            if (ModelState.IsValid)
            {
                try
                {
                    var selectedEvent = await _context.Events.FindAsync(booking.EventId);

                    if (selectedEvent != null)
                    {
                        var conflictingBooking = await _context.Bookings
                            .Include(b => b.Event)
                            .Where(b => b.VenueId == booking.VenueId && b.Id != booking.Id)
                            .Where(b => b.Event.StartDate < selectedEvent.EndDate
                                    && b.Event.EndDate > selectedEvent.StartDate)
                            .FirstOrDefaultAsync();

                        if (conflictingBooking != null)
                        {
                            TempData["ErrorMessage"] = "This venue is already booked for the selected date range.";
                            PopulateDropdowns(booking);
                            return View(booking);
                        }
                    }

                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdowns(booking);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Booking deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
}