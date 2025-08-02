using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusBuddy.Core.Models
{
    [Table("Vehicles")]
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        // Add VehicleId property for compatibility
        [NotMapped]
        public int VehicleId
        {
            get => Id;
            set => Id = value;
        }

        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;

        /// <summary>
        /// License plate number (alias for PlateNumber for compatibility)
        /// </summary>
        [NotMapped]
        public string LicensePlate
        {
            get => PlateNumber;
            set => PlateNumber = value;
        }

        public int Capacity { get; set; }

        /// <summary>
        /// Bus number for identification
        /// </summary>
        public string BusNumber { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the vehicle is active in the system
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Current operational status of the vehicle
        /// </summary>
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Operational status alias for compatibility
        /// </summary>
        public string OperationalStatus
        {
            get => Status;
            set => Status = value;
        }
    }
}
