using System;
using System.ComponentModel.DataAnnotations;
using DeviceManagement.Models.ValidationAttributes;

namespace DeviceManagement.Models
{
    /// <summary>
    /// Represents a hardware device tracked in the inventory.
    /// Meets all assignment requirements: Name, Device Code, Category, Status, Date of Entry.
    /// </summary>
    public class Device
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Device name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Device name must be between 2 and 100 characters.")]
        [Display(Name = "Device Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Device code is required.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Device code must be between 3 and 30 characters.")]
        [RegularExpression(@"^[A-Za-z0-9\-_]+$", ErrorMessage = "Device code may only contain letters, numbers, hyphens, and underscores.")]
        [Display(Name = "Device Code")]
        public string DeviceCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a device category.")]
        [Display(Name = "Category")]
        public int DeviceCategoryId { get; set; }

        /// <summary>
        /// Navigation property to the parent category.
        /// </summary>
        [Display(Name = "Category")]
        public DeviceCategory? DeviceCategory { get; set; }

        [Required(ErrorMessage = "Please select a status.")]
        [Display(Name = "Status")]
        public DeviceStatus Status { get; set; } = DeviceStatus.InUse;

        [Required(ErrorMessage = "Date of entry is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Entry")]
        [NotInFuture(ErrorMessage = "Date of entry cannot be in the future.")]
        public DateTime DateOfEntry { get; set; } = DateTime.Today;
    }
}
