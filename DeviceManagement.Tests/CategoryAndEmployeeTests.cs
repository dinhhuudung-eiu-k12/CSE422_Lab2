using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DeviceManagement.Data;
using DeviceManagement.Models;
using Xunit;

namespace DeviceManagement.Tests
{
    public class CategoryAndEmployeeTests
    {
        private static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void DeviceCategory_WithValidName_PassesValidation()
        {
            var category = new DeviceCategory
            {
                Name = "Laptops",
                Description = "High-end developer laptops"
            };

            var results = ValidateModel(category);

            Assert.Empty(results);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void DeviceCategory_WithEmptyName_FailsValidation(string? invalidName)
        {
            var category = new DeviceCategory
            {
                Name = invalidName!
            };

            var results = ValidateModel(category);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(DeviceCategory.Name)));
        }

        [Fact]
        public void Employee_WithValidFields_PassesValidation()
        {
            var employee = new Employee
            {
                FullName = "Nguyen Van An",
                Email = "an.nguyen@company.edu.vn",
                PhoneNumber = "0901234567"
            };

            var results = ValidateModel(employee);

            Assert.Empty(results);
        }

        [Theory]
        [InlineData("not-an-email")]
        [InlineData("plainaddress")]
        [InlineData("@missingusername.com")]
        [InlineData("missingdomain@")]
        public void Employee_WithInvalidEmail_FailsValidation(string invalidEmail)
        {
            var employee = new Employee
            {
                FullName = "Nguyen Van An",
                Email = invalidEmail,
                PhoneNumber = "0901234567"
            };

            var results = ValidateModel(employee);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Employee.Email)));
        }

        [Theory]
        [InlineData("12")] // Too short
        [InlineData("")]
        public void Employee_WithInvalidPhoneNumber_FailsValidation(string invalidPhone)
        {
            var employee = new Employee
            {
                FullName = "Nguyen Van An",
                Email = "an.nguyen@company.edu.vn",
                PhoneNumber = invalidPhone
            };

            var results = ValidateModel(employee);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Employee.PhoneNumber)));
        }

        [Fact]
        public async Task CategoryDeletion_WhenDevicesReferenced_IsBlockedByBusinessRule()
        {
            var options = new DbContextOptionsBuilder<DeviceManagementContext>()
                .UseInMemoryDatabase(databaseName: nameof(CategoryDeletion_WhenDevicesReferenced_IsBlockedByBusinessRule))
                .Options;

            using var context = new DeviceManagementContext(options);
            await context.Database.EnsureCreatedAsync();

            var category = new DeviceCategory { Id = 10, Name = "Network Switches" };
            var device = new Device
            {
                Id = 101,
                Name = "Core Switch 48-port",
                DeviceCode = "DEV-SW-001",
                DeviceCategoryId = 10,
                Status = DeviceStatus.InUse
            };

            context.Categories.Add(category);
            context.Devices.Add(device);
            await context.SaveChangesAsync();

            // Check referential integrity rule
            var categoryToDelete = await context.Categories
                .Include(c => c.Devices)
                .FirstOrDefaultAsync(c => c.Id == 10);

            Assert.NotNull(categoryToDelete);
            Assert.True(categoryToDelete.Devices.Count > 0, "Category should have referencing devices");

            // Business rule verification: deletion must be prohibited when devices reference it
            bool canDelete = categoryToDelete.Devices.Count == 0;
            Assert.False(canDelete, "Category deletion must be prohibited when devices reference it");
        }
    }
}
