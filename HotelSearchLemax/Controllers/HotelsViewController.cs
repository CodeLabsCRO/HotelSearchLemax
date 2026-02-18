using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HotelSearchLemax.Core.Entities;
using HotelSearchLemax.DataAccess.Data;
using HotelSearchLemax.Models;

namespace HotelSearchLemax.Controllers
{
    public class HotelsViewController : Controller
    {
        private readonly HotelDbContext _context;

        public HotelsViewController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: HotelsView
        public async Task<IActionResult> Index(
            string? searchTerm,
            string sortBy = "Name",
            string sortDirection = "Asc",
            int pageNumber = 1,
            int pageSize = 10)
        {
            // Start with IQueryable - all operations translate to SQL
            IQueryable<Hotel> query = _context.Hotels;

            // Search filter (translated to SQL WHERE LIKE)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(h => h.Name.Contains(searchTerm));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "price" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(h => h.Price)
                    : query.OrderBy(h => h.Price),
                "latitude" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(h => h.Latitude)
                    : query.OrderBy(h => h.Latitude),
                "longitude" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(h => h.Longitude)
                    : query.OrderBy(h => h.Longitude),
                "id" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(h => h.Id)
                    : query.OrderBy(h => h.Id),
                _ => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(h => h.Name)
                    : query.OrderBy(h => h.Name)
            };

            // Apply pagination
            var hotels = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new HotelIndexViewModel
            {
                Hotels = hotels,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortDirection = sortDirection,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(viewModel);
        }

        // GET: HotelsView/Search
        public IActionResult Search()
        {
            return View();
        }

        // GET: HotelsView/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hotel == null)
            {
                return NotFound();
            }

            return View(hotel);
        }

        // GET: HotelsView/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: HotelsView/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Price,Latitude,Longitude,Id")] Hotel hotel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hotel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(hotel);
        }

        // GET: HotelsView/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }
            return View(hotel);
        }

        // POST: HotelsView/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Price,Latitude,Longitude,Id")] Hotel hotel)
        {
            if (id != hotel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hotel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HotelExists(hotel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hotel);
        }

        // GET: HotelsView/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hotel == null)
            {
                return NotFound();
            }

            return View(hotel);
        }

        // POST: HotelsView/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel != null)
            {
                _context.Hotels.Remove(hotel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HotelExists(int id)
        {
            return _context.Hotels.Any(e => e.Id == id);
        }
    }
}
