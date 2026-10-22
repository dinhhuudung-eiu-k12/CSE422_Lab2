using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceManagement.Data;
using DeviceManagement.Models;

namespace DeviceManagement.Controllers
{
    /// <summary>
    /// Handles full CRUD workflows for Device Categories.
    /// Enforces business rules: prevents duplicate category names and blocks deletion when referenced by devices.
    /// </summary>
    public class DeviceCategoriesController : Controller
    {
        private readonly DeviceManagementContext _context;

        public DeviceCategoriesController(DeviceManagementContext context)
        {
            _context = context;
        }

        // GET: DeviceCategories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Devices)
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // GET: DeviceCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Devices)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: DeviceCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DeviceCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] DeviceCategory category)
        {
            // Check for duplicate category name (case-insensitive)
            if (!string.IsNullOrWhiteSpace(category.Name))
            {
                var normalizedName = category.Name.Trim();
                bool exists = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == normalizedName.ToLower());

                if (exists)
                {
                    ModelState.AddModelError("Name", $"A category named '{normalizedName}' already exists.");
                }
            }

            if (ModelState.IsValid)
            {
                category.Name = category.Name.Trim();
                category.Description = category.Description?.Trim();

                _context.Add(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Category '{category.Name}' was successfully created.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: DeviceCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: DeviceCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] DeviceCategory category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            // Check for duplicate category name among other records
            if (!string.IsNullOrWhiteSpace(category.Name))
            {
                var normalizedName = category.Name.Trim();
                bool exists = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == normalizedName.ToLower() && c.Id != id);

                if (exists)
                {
                    ModelState.AddModelError("Name", $"Another category named '{normalizedName}' already exists.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.Name = category.Name.Trim();
                    category.Description = category.Description?.Trim();

                    _context.Update(category);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Category '{category.Name}' was successfully updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CategoryExistsAsync(category.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: DeviceCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Devices)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            // Pass flag to view indicating whether category contains active devices
            ViewBag.HasReferencingDevices = category.Devices.Count > 0;
            ViewBag.DeviceCount = category.Devices.Count;

            return View(category);
        }

        // POST: DeviceCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Devices)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            // Enforce business rule: Cannot delete category if devices reference it
            if (category.Devices.Count > 0)
            {
                TempData["ErrorMessage"] = $"Cannot delete category '{category.Name}' because it currently contains {category.Devices.Count} active device(s). Please reassign or delete these devices first.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Category '{category.Name}' was successfully deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CategoryExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(e => e.Id == id);
        }
    }
}
