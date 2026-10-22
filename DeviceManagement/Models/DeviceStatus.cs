using System.ComponentModel.DataAnnotations;

namespace DeviceManagement.Models
{
    /// <summary>
    /// Represents the operational status of a hardware device.
    /// Exactly the three allowed values specified in the assignment requirements.
    /// </summary>
    public enum DeviceStatus
    {
        [Display(Name = "In Use")]
        InUse = 1,

        [Display(Name = "Broken")]
        Broken = 2,

        [Display(Name = "Under Maintenance")]
        UnderMaintenance = 3
    }
}
