﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using musicShop.Models;

namespace musicShop.Controllers
{
    public class PerformancesController : Controller
    {
        private readonly AppDbContext _context;

        public PerformancesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Performances
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Performances.Include(p => p.Composition).Include(p => p.Ensemble);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Performances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Performances == null)
            {
                return NotFound();
            }

            var performance = await _context.Performances
                .Include(p => p.Composition)
                .Include(p => p.Ensemble)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (performance == null)
            {
                return NotFound();
            }

            return View(performance);
        }

        // GET: Performances/Create
        public IActionResult Create()
        {
            ViewData["CompositionId"] = new SelectList(_context.Compositions, "Id", "Name");
            ViewData["EnsembleId"] = new SelectList(_context.Ensembles, "Id", "Name");
            return View();
        }

        // POST: Performances/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,EnsembleId,CompositionId")] Performance performance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(performance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompositionId"] = new SelectList(_context.Compositions, "Id", "Name", performance.CompositionId);
            ViewData["EnsembleId"] = new SelectList(_context.Ensembles, "Id", "Name", performance.EnsembleId);
            return View(performance);
        }

        // GET: Performances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Performances == null)
            {
                return NotFound();
            }

            var performance = await _context.Performances.FindAsync(id);
            if (performance == null)
            {
                return NotFound();
            }
            ViewData["CompositionId"] = new SelectList(_context.Compositions, "Id", "Name", performance.CompositionId);
            ViewData["EnsembleId"] = new SelectList(_context.Ensembles, "Id", "Name", performance.EnsembleId);
            return View(performance);
        }

        // POST: Performances/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,EnsembleId,CompositionId")] Performance performance)
        {
            if (id != performance.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(performance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PerformanceExists(performance.Id))
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
            ViewData["CompositionId"] = new SelectList(_context.Compositions, "Id", "Name", performance.CompositionId);
            ViewData["EnsembleId"] = new SelectList(_context.Ensembles, "Id", "Name", performance.EnsembleId);
            return View(performance);
        }

        // GET: Performances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Performances == null)
            {
                return NotFound();
            }

            var performance = await _context.Performances
                .Include(p => p.Composition)
                .Include(p => p.Ensemble)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (performance == null)
            {
                return NotFound();
            }

            return View(performance);
        }

        // POST: Performances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Performances == null)
            {
                return Problem("Entity set 'AppDbContext.Performances'  is null.");
            }
            var performance = await _context.Performances.FindAsync(id);
            if (performance != null)
            {
                _context.Performances.Remove(performance);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PerformanceExists(int id)
        {
          return _context.Performances.Any(e => e.Id == id);
        }
    }
}