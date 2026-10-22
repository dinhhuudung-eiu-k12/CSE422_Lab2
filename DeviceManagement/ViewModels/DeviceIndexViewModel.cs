using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using DeviceManagement.Models;

namespace DeviceManagement.ViewModels
{
    /// <summary>
    /// View model for the Devices Index page supporting server-side search and filters.
    /// Preserves query parameters across requests.
    /// </summary>
    public class DeviceIndexViewModel
    {
        public IEnumerable<Device> Devices { get; set; } = new List<Device>();

        public string? SearchString { get; set; }

        public DeviceStatus? Status { get; set; }

        public int? CategoryId { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> Statuses { get; set; } = new List<SelectListItem>();

        public int TotalCount { get; set; }

        public bool HasActiveFilters => !string.IsNullOrWhiteSpace(SearchString) || Status.HasValue || (CategoryId.HasValue && CategoryId.Value > 0);
    }
}
