using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DeviceManagement.Data;
using DeviceManagement.Models;
using DeviceManagement.ViewModels;

namespace DeviceManagement.Controllers
{
    /// <summary>
    /// Handles full CRUD workflows, server-side search, and multi-parameter filtering for Devices.
    /// Demonstrates EF Core query composition, eager loading, and validation.
    /// </summary>
    public class DevicesController : Controller
    {
        private readonly DeviceManagementContext _context;

        public DevicesController(DeviceManagementContext context)
        {
            _context = context;
        }

        // GET: Devices
        // Supports combined search by name/code and filtering by category & status
        public async Task<IActionResult> Index(string? searchString, DeviceStatus? status, int? categoryId)
        {
            // Build base EF Core query with eager loading for the category
            var query = _context.Devices
                .Include(d => d.DeviceCategory)
                .AsNoTracking()
                .AsQueryable();

            // 1. Filter by Search String (matches Device Name OR Device Code, case-insensitive)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var trimmed = searchString.Trim();
                query = query.Where(d => d.Name.Contains(trimmed) || d.DeviceCode.Contains(trimmed));
            }

            // 2. Filter by Device Status
            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status.Value);
            }

            // 3. Filter by Device Category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(d => d.DeviceCategoryId == categoryId.Value);
            }

            // Order by most recently registered devices first
            var filteredDevices = await query
                .OrderByDescending(d => d.DateOfEntry)
                .ThenBy(d => d.Name)
                .ToListAsync();

            // Prepare lookup lists for filter dropdowns
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = categoryId.HasValue && c.Id == categoryId.Value
                })
                .ToListAsync();

            var statuses = Enum.GetValues<DeviceStatus>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = GetStatusDisplayName(s),
                    Selected = status.HasValue && s == status.Value
                })
                .ToList();

            var viewModel = new DeviceIndexViewModel
            {
                Devices = filteredDevices,
                SearchString = searchString,
                Status = status,
                CategoryId = categoryId,
                Categories = categories,
                Statuses = statuses,
                TotalCount = filteredDevices.Count
            };

            return View(viewModel);
        }

        // GET: Devices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var device = await _context.Devices
                .Include(d => d.DeviceCategory)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (device == null)
            {
                return NotFound();
            }

            return View(device);
        }

        // GET: Devices/Create
        public async Task<IActionResult> Create(int? categoryId)
        {
            await PopulateCategoriesDropDownListAsync(categoryId);

            var model = new Device
            {
                DateOfEntry = DateTime.Today,
                Status = DeviceStatus.InUse,
                DeviceCategoryId = categoryId ?? 0
            };

            return View(model);
        }

        // POST: Devices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,DeviceCode,DeviceCategoryId,Status,DateOfEntry")] Device device)
        {
            // 1. Business rule validation: Device Code must be unique (case-insensitive)
            if (!string.IsNullOrWhiteSpace(device.DeviceCode))
            {
                var normalizedCode = device.DeviceCode.Trim().ToUpper();
                bool codeExists = await _context.Devices
                    .AnyAsync(d => d.DeviceCode.ToUpper() == normalizedCode);

                if (codeExists)
                {
                    ModelState.AddModelError("DeviceCode", $"A device with code '{normalizedCode}' already exists.");
                }
            }

            // 2. Business rule validation: DateOfEntry cannot be in the future
            if (device.DateOfEntry.Date > DateTime.Today)
            {
                ModelState.AddModelError("DateOfEntry", "Date of entry cannot be in the future.");
            }

            // 3. Ensure chosen category exists
            bool categoryValid = await _context.Categories.AnyAsync(c => c.Id == device.DeviceCategoryId);
            if (!categoryValid)
            {
                ModelState.AddModelError("DeviceCategoryId", "The selected category does not exist.");
            }

            if (ModelState.IsValid)
            {
                device.Name = device.Name.Trim();
                device.DeviceCode = device.DeviceCode.Trim().ToUpper();

                _context.Add(device);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Device '{device.Name}' ({device.DeviceCode}) was successfully registered.";
                return RedirectToAction(nameof(Index));
            }

            // Redisplay form with validation messages and reloaded dropdown list
            await PopulateCategoriesDropDownListAsync(device.DeviceCategoryId);
            return View(device);
        }

        // GET: Devices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var device = await _context.Devices.FindAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            await PopulateCategoriesDropDownListAsync(device.DeviceCategoryId);
            return View(device);
        }

        // POST: Devices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DeviceCode,DeviceCategoryId,Status,DateOfEntry")] Device device)
        {
            if (id != device.Id)
            {
                return NotFound();
            }

            // 1. Business rule validation: Device Code must be unique among other devices
            if (!string.IsNullOrWhiteSpace(device.DeviceCode))
            {
                var normalizedCode = device.DeviceCode.Trim().ToUpper();
                bool codeExists = await _context.Devices
                    .AnyAsync(d => d.DeviceCode.ToUpper() == normalizedCode && d.Id != id);

                if (codeExists)
                {
                    ModelState.AddModelError("DeviceCode", $"Another device with code '{normalizedCode}' already exists.");
                }
            }

            // 2. Business rule validation: DateOfEntry cannot be in the future
            if (device.DateOfEntry.Date > DateTime.Today)
            {
                ModelState.AddModelError("DateOfEntry", "Date of entry cannot be in the future.");
            }

            // 3. Ensure chosen category exists
            bool categoryValid = await _context.Categories.AnyAsync(c => c.Id == device.DeviceCategoryId);
            if (!categoryValid)
            {
                ModelState.AddModelError("DeviceCategoryId", "The selected category does not exist.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    device.Name = device.Name.Trim();
                    device.DeviceCode = device.DeviceCode.Trim().ToUpper();

                    _context.Update(device);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Device '{device.Name}' ({device.DeviceCode}) was successfully updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await DeviceExistsAsync(device.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateCategoriesDropDownListAsync(device.DeviceCategoryId);
            return View(device);
        }

        // GET: Devices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var device = await _context.Devices
                .Include(d => d.DeviceCategory)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (device == null)
            {
                return NotFound();
            }

            return View(device);
        }

        // POST: Devices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device != null)
            {
                _context.Devices.Remove(device);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Device '{device.Name}' ({device.DeviceCode}) was permanently deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategoriesDropDownListAsync(object? selectedCategory = null)
        {
            var categoriesQuery = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Categories = new SelectList(categoriesQuery, "Id", "Name", selectedCategory);
        }

        private async Task<bool> DeviceExistsAsync(int id)
        {
            return await _context.Devices.AnyAsync(e => e.Id == id);
        }

        private static string GetStatusDisplayName(DeviceStatus status) => status switch
        {
            DeviceStatus.InUse => "In Use",
            DeviceStatus.Broken => "Broken",
            DeviceStatus.UnderMaintenance => "Under Maintenance",
            _ => status.ToString()
        };
    }
}
