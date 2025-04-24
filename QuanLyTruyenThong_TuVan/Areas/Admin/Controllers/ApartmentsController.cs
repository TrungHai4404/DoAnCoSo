using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models;

[Area("Admin")]
public class ApartmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ApartmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var apartments = await _context.Apartments.Include(a => a.Residents).ToListAsync();
        return View(apartments);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Apartment apartment)
    {
        if (ModelState.IsValid)
        {
            _context.Add(apartment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(apartment);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var apartment = await _context.Apartments.FindAsync(id);
        if (apartment == null) return NotFound();
        return View(apartment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Apartment apartment)
    {
        if (id != apartment.Id) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(apartment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(apartment);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var apartment = await _context.Apartments.FindAsync(id);
        if (apartment == null) return NotFound();
        return View(apartment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var apartment = await _context.Apartments.FindAsync(id);
        _context.Apartments.Remove(apartment);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var apartment = await _context.Apartments
            .Include(a => a.Residents)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (apartment == null) return NotFound();
        return View(apartment);
    }
}
