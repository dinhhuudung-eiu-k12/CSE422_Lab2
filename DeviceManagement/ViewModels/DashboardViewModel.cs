using System.Collections.Generic;
using DeviceManagement.Models;

namespace DeviceManagement.ViewModels
{
    /// <summary>
    /// View model for the Dashboard landing page summarizing inventory statistics.
    /// </summary>
    public class DashboardViewModel
    {
        public int TotalDevices { get; set; }
        public int TotalCategories { get; set; }
        public int TotalEmployees { get; set; }
        public int InUseCount { get; set; }
        public int BrokenCount { get; set; }
        public int UnderMaintenanceCount { get; set; }

        public IEnumerable<Device> RecentDevices { get; set; } = new List<Device>();
        public IEnumerable<CategoryDeviceCount> CategoryCounts { get; set; } = new List<CategoryDeviceCount>();
    }

    public class CategoryDeviceCount
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int DeviceCount { get; set; }
    }
}
