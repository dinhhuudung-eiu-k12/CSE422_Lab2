using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DeviceManagement.Models;

namespace DeviceManagement.Data
{
    /// <summary>
    /// Utility class for database initialization and realistic sample data seeding.
    /// Runs idempotently to ensure seed data is only inserted once.
    /// </summary>
    public static class DbInitializer
    {
        public static async Task InitializeAsync(DeviceManagementContext context)
        {
            // Ensure database schema is migrated and up-to-date
            await context.Database.MigrateAsync();

            // Check if data already exists
            if (await context.Categories.AnyAsync())
            {
                return; // Database has already been seeded
            }

            // 1. Seed Categories
            var categories = new DeviceCategory[]
            {
                new DeviceCategory
                {
                    Name = "Laptops",
                    Description = "Business laptops, mobile workstations, and ultrabooks for employees."
                },
                new DeviceCategory
                {
                    Name = "Desktop Computers",
                    Description = "Office workstations, developer desktops, and lab computers."
                },
                new DeviceCategory
                {
                    Name = "Monitors",
                    Description = "External LED displays, dual-monitor setups, and 4K designer screens."
                },
                new DeviceCategory
                {
                    Name = "Printers & Scanners",
                    Description = "Department network laser printers, multifunction scanners, and label printers."
                },
                new DeviceCategory
                {
                    Name = "Mobile Devices",
                    Description = "Company-issued smartphones, test tablets, and handheld inventory scanners."
                },
                new DeviceCategory
                {
                    Name = "Network Equipment",
                    Description = "Managed switches, enterprise Wi-Fi routers, firewalls, and server racks."
                }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            // 2. Seed Devices
            var laptops = categories.First(c => c.Name == "Laptops");
            var desktops = categories.First(c => c.Name == "Desktop Computers");
            var monitors = categories.First(c => c.Name == "Monitors");
            var printers = categories.First(c => c.Name == "Printers & Scanners");
            var mobile = categories.First(c => c.Name == "Mobile Devices");
            var network = categories.First(c => c.Name == "Network Equipment");

            var devices = new Device[]
            {
                new Device
                {
                    Name = "Dell Latitude 5540 15\"",
                    DeviceCode = "DEV-LT-001",
                    DeviceCategoryId = laptops.Id,
                    Status = DeviceStatus.InUse,
                    DateOfEntry = DateTime.Today.AddMonths(-10)
                },
                new Device
                {
                    Name = "ThinkPad T14 Gen 4",
                    DeviceCode = "DEV-LT-002",
                    DeviceCategoryId = laptops.Id,
                    Status = DeviceStatus.InUse,
                    DateOfEntry = DateTime.Today.AddMonths(-8)
                },
                new Device
                {
                    Name = "MacBook Pro 16\" M3",
                    DeviceCode = "DEV-LT-003",
                    DeviceCategoryId = laptops.Id,
                    Status = DeviceStatus.UnderMaintenance,
                    DateOfEntry = DateTime.Today.AddMonths(-3)
                },
                new Device
                {
                    Name = "HP EliteBook 840 G8",
                    DeviceCode = "DEV-LT-004",
                    DeviceCategoryId = laptops.Id,
                    Status = DeviceStatus.Broken,
                    DateOfEntry = DateTime.Today.AddYears(-1)
                },
                new Device
                {
                    Name = "Dell Precision 3660 Tower",
                    DeviceCode = "DEV-DT-001",
                    DeviceCategoryId = desktops.Id,
                    Status = DeviceStatus.InUse,
                    DateOfEntry = DateTime.Today.AddMonths(-14)
                },
                new Device
                {
                    Name = "HP Z4 Workstation",
                    DeviceCode = "DEV-DT-002",
                    DeviceCategoryId = desktops.Id,
                    Status = DeviceStatus.InUse,
                    DateOfEntry = DateTime.Today.AddMonths(-6)
                },
                new Device
                {
                    Name = "Dell UltraSharp 27\" 4K U2723QE",
                    DeviceCode = "DEV-MN-001",
                    DeviceCategoryId = monitors.Id,
                    Status = DeviceStatus.InUse,
                    DateOfEntry = DateTime.Today.AddMonths(-9)
                },
                new Device
                {
                    Name = "LG 34\" Ultrawide 34WN80C",
                    DeviceCode = "DEV-MN-002",
                    DeviceCategoryId = monitors.Id,
                    Status = DeviceStatus.InUse,
                    DateOfEntry = DateTime.Today.AddMonths(-5)
                },
                new Device
                {
                    Name = "Samsung 24\" FHD Essential",
                    DeviceCode = "DEV-MN-003",
                    DeviceCategoryId = monitors.Id,
                    Status = DeviceStatus.Broken,
                    DateOfEntry = DateTime.Today.AddYears(-2)
                },
                new Device
                {
                    Name = "HP LaserJet Enterprise M507x",
                    DeviceCode = "DEV-PR-001",
                    DeviceCategoryId = printers.Id,
                    Status = DeviceStatus.InUse,
                    DateOfEntry = DateTime.Today.AddMonths(-12)
                },
                new Device
                {
                    Name = "Canon imageCLASS MF445dw",
                    DeviceCode = "DEV-PR-002",
                    DeviceCategoryId = printers.Id,
                    Status = DeviceStatus.UnderMaintenance,
                    DateOfEntry = DateTime.Today.AddMonths(-4)
                },
                new Device
                {
                    Name = "Apple iPad Air 10.9\" 64GB",
                    DeviceCode = "DEV-MB-001",
                    DeviceCategoryId = mobile.Id,
                    Status = DeviceStatus.InUse,
                    DateOfEntry = DateTime.Today.AddMonths(-7)
                },
                new Device
                {
                    Name = "Samsung Galaxy Tab Active4 Pro",
                    DeviceCode = "DEV-MB-002",
                    DeviceCategoryId = mobile.Id,
                    Status = DeviceStatus.InUse,
                    DateOfEntry = DateTime.Today.AddMonths(-2)
                },
                new Device
                {
                    Name = "Cisco Catalyst 2960-X Switch",
                    DeviceCode = "DEV-NW-001",
                    DeviceCategoryId = network.Id,
                    Status = DeviceStatus.InUse,
                    DateOfEntry = DateTime.Today.AddYears(-2)
                },
                new Device
                {
                    Name = "Ubiquiti UniFi Pro Access Point",
                    DeviceCode = "DEV-NW-002",
                    DeviceCategoryId = network.Id,
                    Status = DeviceStatus.InUse,
                    DateOfEntry = DateTime.Today.AddMonths(-11)
                }
            };

            await context.Devices.AddRangeAsync(devices);

            // 3. Seed Employees
            var employees = new Employee[]
            {
                new Employee
                {
                    FullName = "Nguyen Van An",
                    Email = "an.nguyen@company.edu.vn",
                    PhoneNumber = "0901234567"
                },
                new Employee
                {
                    FullName = "Tran Thi Mai",
                    Email = "mai.tran@company.edu.vn",
                    PhoneNumber = "0912345678"
                },
                new Employee
                {
                    FullName = "Le Hoang Nam",
                    Email = "nam.le@company.edu.vn",
                    PhoneNumber = "0923456789"
                },
                new Employee
                {
                    FullName = "Pham Minh Duc",
                    Email = "duc.pham@company.edu.vn",
                    PhoneNumber = "0934567890"
                },
                new Employee
                {
                    FullName = "Vo Thi Huong",
                    Email = "huong.vo@company.edu.vn",
                    PhoneNumber = "0945678901"
                }
            };

            await context.Employees.AddRangeAsync(employees);
            await context.SaveChangesAsync();
        }
    }
}
