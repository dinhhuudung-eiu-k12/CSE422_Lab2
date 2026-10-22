using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DeviceManagement.Data;
using DeviceManagement.Models;
using Xunit;

namespace DeviceManagement.Tests
{
    public class DeviceFilterTests
    {
        private async Task<DeviceManagementContext> GetInMemoryContextAsync(string dbName)
        {
            var options = new DbContextOptionsBuilder<DeviceManagementContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new DeviceManagementContext(options);
            await context.Database.EnsureCreatedAsync();

            if (!context.Categories.Any())
            {
                var laptopCat = new DeviceCategory { Id = 1, Name = "Laptops" };
                var monitorCat = new DeviceCategory { Id = 2, Name = "Monitors" };
                var printerCat = new DeviceCategory { Id = 3, Name = "Printers" };

                context.Categories.AddRange(laptopCat, monitorCat, printerCat);

                context.Devices.AddRange(
                    new Device
                    {
                        Id = 1,
                        Name = "Dell Latitude 5540",
                        DeviceCode = "DEV-LT-001",
                        DeviceCategoryId = 1,
                        Status = DeviceStatus.InUse,
                        DateOfEntry = DateTime.Today.AddMonths(-3)
                    },
                    new Device
                    {
                        Id = 2,
                        Name = "MacBook Pro 16",
                        DeviceCode = "DEV-LT-002",
                        DeviceCategoryId = 1,
                        Status = DeviceStatus.UnderMaintenance,
                        DateOfEntry = DateTime.Today.AddMonths(-2)
                    },
                    new Device
                    {
                        Id = 3,
                        Name = "Dell UltraSharp 27",
                        DeviceCode = "DEV-MN-001",
                        DeviceCategoryId = 2,
                        Status = DeviceStatus.InUse,
                        DateOfEntry = DateTime.Today.AddMonths(-5)
                    },
                    new Device
                    {
                        Id = 4,
                        Name = "HP LaserJet Enterprise",
                        DeviceCode = "DEV-PR-001",
                        DeviceCategoryId = 3,
                        Status = DeviceStatus.Broken,
                        DateOfEntry = DateTime.Today.AddMonths(-8)
                    }
                );

                await context.SaveChangesAsync();
            }

            return context;
        }

        [Fact]
        public async Task SearchDevices_ByName_ReturnsMatchingDevices()
        {
            using var context = await GetInMemoryContextAsync(nameof(SearchDevices_ByName_ReturnsMatchingDevices));

            var search = "Latitude";
            var results = await context.Devices
                .Where(d => d.Name.Contains(search) || d.DeviceCode.Contains(search))
                .ToListAsync();

            Assert.Single(results);
            Assert.Equal("DEV-LT-001", results[0].DeviceCode);
        }

        [Fact]
        public async Task SearchDevices_ByCode_ReturnsMatchingDevices()
        {
            using var context = await GetInMemoryContextAsync(nameof(SearchDevices_ByCode_ReturnsMatchingDevices));

            var search = "DEV-MN";
            var results = await context.Devices
                .Where(d => d.Name.Contains(search) || d.DeviceCode.Contains(search))
                .ToListAsync();

            Assert.Single(results);
            Assert.Equal("Dell UltraSharp 27", results[0].Name);
        }

        [Fact]
        public async Task FilterDevices_ByStatus_ReturnsOnlyMatchingStatus()
        {
            using var context = await GetInMemoryContextAsync(nameof(FilterDevices_ByStatus_ReturnsOnlyMatchingStatus));

            var results = await context.Devices
                .Where(d => d.Status == DeviceStatus.InUse)
                .ToListAsync();

            Assert.Equal(2, results.Count);
            Assert.All(results, d => Assert.Equal(DeviceStatus.InUse, d.Status));
        }

        [Fact]
        public async Task FilterDevices_ByCategory_ReturnsOnlyCategoryDevices()
        {
            using var context = await GetInMemoryContextAsync(nameof(FilterDevices_ByCategory_ReturnsOnlyCategoryDevices));

            var results = await context.Devices
                .Where(d => d.DeviceCategoryId == 1) // Laptops
                .ToListAsync();

            Assert.Equal(2, results.Count);
            Assert.All(results, d => Assert.Equal(1, d.DeviceCategoryId));
        }

        [Fact]
        public async Task CombinedSearchAndFilter_ReturnsPreciseMatch()
        {
            using var context = await GetInMemoryContextAsync(nameof(CombinedSearchAndFilter_ReturnsPreciseMatch));

            var search = "Dell";
            var categoryId = 1; // Laptops
            var status = DeviceStatus.InUse;

            var query = context.Devices.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => d.Name.Contains(search) || d.DeviceCode.Contains(search));
            }
            if (categoryId > 0)
            {
                query = query.Where(d => d.DeviceCategoryId == categoryId);
            }
            if (status != 0)
            {
                query = query.Where(d => d.Status == status);
            }

            var results = await query.ToListAsync();

            Assert.Single(results);
            Assert.Equal("DEV-LT-001", results[0].DeviceCode);
            Assert.Equal("Dell Latitude 5540", results[0].Name);
        }
    }
}
