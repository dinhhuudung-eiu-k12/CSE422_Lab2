using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceManagement.Data;
using DeviceManagement.Models;
using DeviceManagement.ViewModels;

namespace DeviceManagement.Controllers
{
    /// <summary>
    /// Controller for the dashboard overview and general application pages.
    /// Injects DeviceManagementContext via constructor dependency injection.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly DeviceManagementContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(DeviceManagementContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /
        // GET: /Home/Index
        public async Task<IActionResult> Index()
        {
            var totalDevices = await _context.Devices.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalEmployees = await _context.Employees.CountAsync();

            var inUseCount = await _context.Devices.CountAsync(d => d.Status == DeviceStatus.InUse);
            var brokenCount = await _context.Devices.CountAsync(d => d.Status == DeviceStatus.Broken);
            var underMaintenanceCount = await _context.Devices.CountAsync(d => d.Status == DeviceStatus.UnderMaintenance);

            var recentDevices = await _context.Devices
                .Include(d => d.DeviceCategory)
                .AsNoTracking()
                .OrderByDescending(d => d.DateOfEntry)
                .Take(6)
                .ToListAsync();

            var categoryCounts = await _context.Categories
                .AsNoTracking()
                .Select(c => new CategoryDeviceCount
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    DeviceCount = c.Devices.Count
                })
                .OrderByDescending(c => c.DeviceCount)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalDevices = totalDevices,
                TotalCategories = totalCategories,
                TotalEmployees = totalEmployees,
                InUseCount = inUseCount,
                BrokenCount = brokenCount,
                UnderMaintenanceCount = underMaintenanceCount,
                RecentDevices = recentDevices,
                CategoryCounts = categoryCounts
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
