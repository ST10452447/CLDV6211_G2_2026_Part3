using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;
using EventEase.ViewModels;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobStorageService;

        public EventsController(ApplicationDbContext context, BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            return View(await _context.Events.ToListAsync());
        }

        // ★★★★★ ADVANCED SEARCH WITH FILTERS ★★★★★
        [HttpGet]
        public async Task<IActionResult> AdvancedSearch(AdvancedSearchViewModel model)
        {
            // Build query with includes
            var eventsQuery = _context.Events
                .Include(e => e.EventType)
                .Include(e => e.Bookings)
                .AsQueryable();

            // 1. Filter by Event Type
            if (model.EventTypeId.HasValue && model.EventTypeId > 0)
            {
                eventsQuery = eventsQuery.Where(e => e.EventTypeId == model.EventTypeId);
            }

            // 2. Filter by Start Date
            if (model.StartDate.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.StartDate >= model.StartDate);
            }

            // 3. Filter by End Date
            if (model.EndDate.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.EndDate <= model.EndDate);
            }

            // 4. Filter by Availability (only events with no bookings)
            if (model.ShowOnlyAvailable)
            {
                eventsQuery = eventsQuery.Where(e => !e.Bookings.Any());
            }

            // Execute query
            model.Events = await eventsQuery.ToListAsync();

            // Populate EventTypes dropdown
            model.EventTypes = await _context.EventTypes.ToListAsync();

            return View(model);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null) return NotFound();

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,EndDate,EventTypeId,ImageFile")] Event @event)
        {
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                if (@event.ImageFile != null)
                {
                    @event.ImageUrl = await _blobStorageService.UploadImageAsync(@event.ImageFile);
                }

                _context.Add(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,StartDate,EndDate,EventTypeId,ImageUrl,ImageFile")] Event @event)
        {
            if (id != @event.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (@event.ImageFile != null)
                    {
                        @event.ImageUrl = await _blobStorageService.UploadImageAsync(@event.ImageFile);
                    }

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null) return NotFound();

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (@event != null)
            {
                if (@event.Bookings != null && @event.Bookings.Any())
                {
                    TempData["ErrorMessage"] = "❌ Cannot delete this event because it has active bookings. Please delete the bookings first.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "✅ Event deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Event not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}