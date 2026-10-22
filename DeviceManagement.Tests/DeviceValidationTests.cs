using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DeviceManagement.Models;
using DeviceManagement.Models.ValidationAttributes;
using Xunit;

namespace DeviceManagement.Tests
{
    public class DeviceValidationTests
    {
        private static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void NotInFutureAttribute_WithPastDate_ReturnsSuccess()
        {
            var attribute = new NotInFutureAttribute();
            var pastDate = DateTime.Today.AddDays(-7);

            var result = attribute.GetValidationResult(pastDate, new ValidationContext(new object()));

            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void NotInFutureAttribute_WithTodayDate_ReturnsSuccess()
        {
            var attribute = new NotInFutureAttribute();
            var today = DateTime.Today;

            var result = attribute.GetValidationResult(today, new ValidationContext(new object()));

            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void NotInFutureAttribute_WithFutureDate_ReturnsValidationError()
        {
            var attribute = new NotInFutureAttribute();
            var futureDate = DateTime.Today.AddDays(1);

            var result = attribute.GetValidationResult(futureDate, new ValidationContext(new object()));

            Assert.NotNull(result);
            Assert.Equal("Date of entry cannot be in the future.", result?.ErrorMessage);
        }

        [Fact]
        public void Device_WithValidFields_PassesValidation()
        {
            var device = new Device
            {
                Name = "Dell Latitude 5540",
                DeviceCode = "DEV-LT-001",
                DeviceCategoryId = 1,
                Status = DeviceStatus.InUse,
                DateOfEntry = DateTime.Today.AddDays(-10)
            };

            var results = ValidateModel(device);

            Assert.Empty(results);
        }

        [Fact]
        public void Device_WithFutureDate_FailsValidation()
        {
            var device = new Device
            {
                Name = "Dell Latitude 5540",
                DeviceCode = "DEV-LT-001",
                DeviceCategoryId = 1,
                Status = DeviceStatus.InUse,
                DateOfEntry = DateTime.Today.AddDays(5)
            };

            var results = ValidateModel(device);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Device.DateOfEntry)));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Device_WithEmptyName_FailsValidation(string? invalidName)
        {
            var device = new Device
            {
                Name = invalidName!,
                DeviceCode = "DEV-001",
                DeviceCategoryId = 1,
                Status = DeviceStatus.InUse,
                DateOfEntry = DateTime.Today
            };

            var results = ValidateModel(device);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Device.Name)));
        }

        [Theory]
        [InlineData("AB")] // Too short (min 3)
        [InlineData("DEV@#01")] // Invalid characters
        [InlineData("DEV CODE WITH SPACES")]
        public void Device_WithInvalidDeviceCode_FailsValidation(string invalidCode)
        {
            var device = new Device
            {
                Name = "Test Monitor",
                DeviceCode = invalidCode,
                DeviceCategoryId = 1,
                Status = DeviceStatus.InUse,
                DateOfEntry = DateTime.Today
            };

            var results = ValidateModel(device);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Device.DeviceCode)));
        }
    }
}
