using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceManagement.Data;
using DeviceManagement.Models;

namespace DeviceManagement.Controllers
{
    /// <summary>
    /// Handles User / Employee contact management.
    /// Manages organizational user directory without requiring full ASP.NET Identity authentication.
    /// </summary>
    public class EmployeesController : Controller
    {
        private readonly DeviceManagementContext _context;

        public EmployeesController(DeviceManagementContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .AsNoTracking()
                .OrderBy(e => e.FullName)
                .ToListAsync();

            return View(employees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,PhoneNumber")] Employee employee)
        {
            // Business rule: Email address must be unique (case-insensitive)
            if (!string.IsNullOrWhiteSpace(employee.Email))
            {
                var normalizedEmail = employee.Email.Trim().ToLower();
                bool emailExists = await _context.Employees
                    .AnyAsync(e => e.Email.ToLower() == normalizedEmail);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", $"An employee with the email address '{normalizedEmail}' already exists.");
                }
            }

            if (ModelState.IsValid)
            {
                employee.FullName = employee.FullName.Trim();
                employee.Email = employee.Email.Trim().ToLower();
                employee.PhoneNumber = employee.PhoneNumber.Trim();

                _context.Add(employee);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Employee '{employee.FullName}' was successfully added.";
                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,PhoneNumber")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            // Business rule: Email address must be unique among other employees
            if (!string.IsNullOrWhiteSpace(employee.Email))
            {
                var normalizedEmail = employee.Email.Trim().ToLower();
                bool emailExists = await _context.Employees
                    .AnyAsync(e => e.Email.ToLower() == normalizedEmail && e.Id != id);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", $"Another employee with the email address '{normalizedEmail}' already exists.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    employee.FullName = employee.FullName.Trim();
                    employee.Email = employee.Email.Trim().ToLower();
                    employee.PhoneNumber = employee.PhoneNumber.Trim();

                    _context.Update(employee);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Employee '{employee.FullName}' was successfully updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await EmployeeExistsAsync(employee.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Employee '{employee.FullName}' was deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> EmployeeExistsAsync(int id)
        {
            return await _context.Employees.AnyAsync(e => e.Id == id);
        }
    }
}
