using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeviceManagement.Models
{
    /// <summary>
    /// Represents a category to which devices belong (e.g. Computer, Printer, Phone).
    /// </summary>
    public class DeviceCategory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters.")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        /// <summary>
        /// Navigation property: one category has many devices.
        /// </summary>
        public ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}
