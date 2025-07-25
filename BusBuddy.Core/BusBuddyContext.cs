using Microsoft.EntityFrameworkCore;
using BusBuddy.Core.Models;

namespace BusBuddy.Core
{
    public class BusBuddyContext : DbContext
    {
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Activity> Activities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("BusBuddyDb"); // Phase 1: In-memory for simplicity
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed real-world drivers with proper CDL credentials
            modelBuilder.Entity<Driver>().HasData(
                new Driver { DriverId = 1, DriverName = "Michael Rodriguez", DriverPhone = "555-0123", DriverEmail = "m.rodriguez@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 2, DriverName = "Sarah Johnson", DriverPhone = "555-0124", DriverEmail = "s.johnson@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 3, DriverName = "David Chen", DriverPhone = "555-0125", DriverEmail = "d.chen@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 4, DriverName = "Maria Garcia", DriverPhone = "555-0126", DriverEmail = "m.garcia@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 5, DriverName = "James Wilson", DriverPhone = "555-0127", DriverEmail = "j.wilson@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 6, DriverName = "Ashley Brown", DriverPhone = "555-0128", DriverEmail = "a.brown@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 7, DriverName = "Robert Martinez", DriverPhone = "555-0129", DriverEmail = "r.martinez@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 8, DriverName = "Jennifer Lee", DriverPhone = "555-0130", DriverEmail = "j.lee@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 9, DriverName = "Christopher Davis", DriverPhone = "555-0131", DriverEmail = "c.davis@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 10, DriverName = "Amanda Thompson", DriverPhone = "555-0132", DriverEmail = "a.thompson@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 11, DriverName = "Kevin Anderson", DriverPhone = "555-0133", DriverEmail = "k.anderson@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 12, DriverName = "Lisa White", DriverPhone = "555-0134", DriverEmail = "l.white@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 13, DriverName = "Daniel Moore", DriverPhone = "555-0135", DriverEmail = "d.moore@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 14, DriverName = "Nicole Taylor", DriverPhone = "555-0136", DriverEmail = "n.taylor@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 15, DriverName = "Brandon Jackson", DriverPhone = "555-0137", DriverEmail = "b.jackson@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 16, DriverName = "Stephanie Miller", DriverPhone = "555-0138", DriverEmail = "s.miller@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 17, DriverName = "Andrew Harris", DriverPhone = "555-0139", DriverEmail = "a.harris@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 18, DriverName = "Michelle Clark", DriverPhone = "555-0140", DriverEmail = "m.clark@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 19, DriverName = "Ryan Lewis", DriverPhone = "555-0141", DriverEmail = "r.lewis@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" },
                new Driver { DriverId = 20, DriverName = "Samantha Walker", DriverPhone = "555-0142", DriverEmail = "s.walker@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active" }
            );

            // Seed real-world school buses from major manufacturers
            modelBuilder.Entity<Vehicle>().HasData(
                new Vehicle { Id = 1, Make = "Blue Bird", Model = "Vision", PlateNumber = "SCH-001", Capacity = 72 },
                new Vehicle { Id = 2, Make = "Thomas Built", Model = "C2", PlateNumber = "SCH-002", Capacity = 77 },
                new Vehicle { Id = 3, Make = "IC Bus", Model = "CE200", PlateNumber = "SCH-003", Capacity = 71 },
                new Vehicle { Id = 4, Make = "Blue Bird", Model = "All American RE", PlateNumber = "SCH-004", Capacity = 84 },
                new Vehicle { Id = 5, Make = "Thomas Built", Model = "Saf-T-Liner HDX", PlateNumber = "SCH-005", Capacity = 90 },
                new Vehicle { Id = 6, Make = "IC Bus", Model = "RE200", PlateNumber = "SCH-006", Capacity = 77 },
                new Vehicle { Id = 7, Make = "Blue Bird", Model = "Vision", PlateNumber = "SCH-007", Capacity = 72 },
                new Vehicle { Id = 8, Make = "Thomas Built", Model = "C2", PlateNumber = "SCH-008", Capacity = 77 },
                new Vehicle { Id = 9, Make = "IC Bus", Model = "CE200", PlateNumber = "SCH-009", Capacity = 71 },
                new Vehicle { Id = 10, Make = "Blue Bird", Model = "All American FE", PlateNumber = "SCH-010", Capacity = 48 },
                new Vehicle { Id = 11, Make = "Thomas Built", Model = "Minotour", PlateNumber = "SCH-011", Capacity = 24 },
                new Vehicle { Id = 12, Make = "IC Bus", Model = "AC200", PlateNumber = "SCH-012", Capacity = 90 },
                new Vehicle { Id = 13, Make = "Blue Bird", Model = "Micro Bird", PlateNumber = "SCH-013", Capacity = 30 },
                new Vehicle { Id = 14, Make = "Thomas Built", Model = "EFX", PlateNumber = "SCH-014", Capacity = 35 },
                new Vehicle { Id = 15, Make = "IC Bus", Model = "TE200", PlateNumber = "SCH-015", Capacity = 77 }
            );

            // Seed realistic school activities and field trips
            var baseDate = DateTime.Today;
            modelBuilder.Entity<Activity>().HasData(
                new Activity { ActivityId = 1, ActivityType = "Field Trip", Destination = "Natural History Museum", Date = baseDate.AddDays(1), LeaveTime = new TimeSpan(8, 30, 0), EventTime = new TimeSpan(10, 0, 0), RequestedBy = "Mrs. Thompson", AssignedVehicleId = 1, DriverId = 1, StudentsCount = 45 },
                new Activity { ActivityId = 2, ActivityType = "Sports Event", Destination = "Central High School", Date = baseDate.AddDays(1), LeaveTime = new TimeSpan(14, 0, 0), EventTime = new TimeSpan(15, 30, 0), RequestedBy = "Coach Martinez", AssignedVehicleId = 2, DriverId = 2, StudentsCount = 22 },
                new Activity { ActivityId = 3, ActivityType = "Academic Competition", Destination = "University Campus", Date = baseDate.AddDays(2), LeaveTime = new TimeSpan(7, 45, 0), EventTime = new TimeSpan(9, 0, 0), RequestedBy = "Mr. Chen", AssignedVehicleId = 3, DriverId = 3, StudentsCount = 15 },
                new Activity { ActivityId = 4, ActivityType = "Field Trip", Destination = "Science Center", Date = baseDate.AddDays(2), LeaveTime = new TimeSpan(9, 15, 0), EventTime = new TimeSpan(10, 30, 0), RequestedBy = "Ms. Garcia", AssignedVehicleId = 4, DriverId = 4, StudentsCount = 38 },
                new Activity { ActivityId = 5, ActivityType = "Band Competition", Destination = "Performing Arts Center", Date = baseDate.AddDays(3), LeaveTime = new TimeSpan(12, 0, 0), EventTime = new TimeSpan(14, 0, 0), RequestedBy = "Mr. Wilson", AssignedVehicleId = 5, DriverId = 5, StudentsCount = 65 },
                new Activity { ActivityId = 6, ActivityType = "Sports Event", Destination = "Regional Stadium", Date = baseDate.AddDays(3), LeaveTime = new TimeSpan(16, 30, 0), EventTime = new TimeSpan(18, 0, 0), RequestedBy = "Coach Brown", AssignedVehicleId = 6, DriverId = 6, StudentsCount = 28 },
                new Activity { ActivityId = 7, ActivityType = "Community Service", Destination = "Local Food Bank", Date = baseDate.AddDays(4), LeaveTime = new TimeSpan(8, 0, 0), EventTime = new TimeSpan(9, 0, 0), RequestedBy = "Mrs. Martinez", AssignedVehicleId = 7, DriverId = 7, StudentsCount = 20 },
                new Activity { ActivityId = 8, ActivityType = "Field Trip", Destination = "Art Museum", Date = baseDate.AddDays(4), LeaveTime = new TimeSpan(10, 45, 0), EventTime = new TimeSpan(12, 0, 0), RequestedBy = "Ms. Lee", AssignedVehicleId = 8, DriverId = 8, StudentsCount = 32 },
                new Activity { ActivityId = 9, ActivityType = "Academic Competition", Destination = "Convention Center", Date = baseDate.AddDays(5), LeaveTime = new TimeSpan(7, 30, 0), EventTime = new TimeSpan(8, 45, 0), RequestedBy = "Mr. Davis", AssignedVehicleId = 9, DriverId = 9, StudentsCount = 12 },
                new Activity { ActivityId = 10, ActivityType = "Sports Event", Destination = "Aquatic Center", Date = baseDate.AddDays(5), LeaveTime = new TimeSpan(13, 15, 0), EventTime = new TimeSpan(15, 0, 0), RequestedBy = "Coach Thompson", AssignedVehicleId = 10, DriverId = 10, StudentsCount = 18 },
                new Activity { ActivityId = 11, ActivityType = "Field Trip", Destination = "Planetarium", Date = baseDate.AddDays(6), LeaveTime = new TimeSpan(9, 30, 0), EventTime = new TimeSpan(11, 0, 0), RequestedBy = "Dr. Anderson", AssignedVehicleId = 11, DriverId = 11, StudentsCount = 25 },
                new Activity { ActivityId = 12, ActivityType = "Drama Performance", Destination = "Community Theater", Date = baseDate.AddDays(6), LeaveTime = new TimeSpan(17, 0, 0), EventTime = new TimeSpan(19, 0, 0), RequestedBy = "Ms. White", AssignedVehicleId = 12, DriverId = 12, StudentsCount = 35 },
                new Activity { ActivityId = 13, ActivityType = "Environmental Study", Destination = "Nature Reserve", Date = baseDate.AddDays(7), LeaveTime = new TimeSpan(8, 15, 0), EventTime = new TimeSpan(9, 30, 0), RequestedBy = "Mr. Moore", AssignedVehicleId = 13, DriverId = 13, StudentsCount = 28 },
                new Activity { ActivityId = 14, ActivityType = "Career Fair", Destination = "Business District", Date = baseDate.AddDays(7), LeaveTime = new TimeSpan(11, 30, 0), EventTime = new TimeSpan(13, 0, 0), RequestedBy = "Ms. Taylor", AssignedVehicleId = 14, DriverId = 14, StudentsCount = 42 },
                new Activity { ActivityId = 15, ActivityType = "Sports Event", Destination = "Golf Course", Date = baseDate.AddDays(8), LeaveTime = new TimeSpan(6, 45, 0), EventTime = new TimeSpan(8, 0, 0), RequestedBy = "Coach Jackson", AssignedVehicleId = 15, DriverId = 15, StudentsCount = 8 },
                new Activity { ActivityId = 16, ActivityType = "Field Trip", Destination = "Historical Society", Date = baseDate.AddDays(8), LeaveTime = new TimeSpan(10, 0, 0), EventTime = new TimeSpan(11, 30, 0), RequestedBy = "Mrs. Miller", AssignedVehicleId = 1, DriverId = 16, StudentsCount = 30 },
                new Activity { ActivityId = 17, ActivityType = "Academic Competition", Destination = "Tech Campus", Date = baseDate.AddDays(9), LeaveTime = new TimeSpan(7, 15, 0), EventTime = new TimeSpan(8, 30, 0), RequestedBy = "Mr. Harris", AssignedVehicleId = 2, DriverId = 17, StudentsCount = 16 },
                new Activity { ActivityId = 18, ActivityType = "Community Event", Destination = "City Hall", Date = baseDate.AddDays(9), LeaveTime = new TimeSpan(14, 30, 0), EventTime = new TimeSpan(16, 0, 0), RequestedBy = "Ms. Clark", AssignedVehicleId = 3, DriverId = 18, StudentsCount = 50 },
                new Activity { ActivityId = 19, ActivityType = "Sports Event", Destination = "Tennis Complex", Date = baseDate.AddDays(10), LeaveTime = new TimeSpan(15, 45, 0), EventTime = new TimeSpan(17, 15, 0), RequestedBy = "Coach Lewis", AssignedVehicleId = 4, DriverId = 19, StudentsCount = 12 },
                new Activity { ActivityId = 20, ActivityType = "Field Trip", Destination = "Botanical Gardens", Date = baseDate.AddDays(10), LeaveTime = new TimeSpan(9, 0, 0), EventTime = new TimeSpan(10, 15, 0), RequestedBy = "Mrs. Walker", AssignedVehicleId = 5, DriverId = 20, StudentsCount = 40 },
                new Activity { ActivityId = 21, ActivityType = "Music Festival", Destination = "Concert Hall", Date = baseDate.AddDays(11), LeaveTime = new TimeSpan(13, 30, 0), EventTime = new TimeSpan(15, 30, 0), RequestedBy = "Mr. Rodriguez", AssignedVehicleId = 6, DriverId = 1, StudentsCount = 55 },
                new Activity { ActivityId = 22, ActivityType = "Science Fair", Destination = "Exhibition Center", Date = baseDate.AddDays(11), LeaveTime = new TimeSpan(8, 45, 0), EventTime = new TimeSpan(10, 0, 0), RequestedBy = "Dr. Johnson", AssignedVehicleId = 7, DriverId = 2, StudentsCount = 33 },
                new Activity { ActivityId = 23, ActivityType = "Sports Event", Destination = "Basketball Arena", Date = baseDate.AddDays(12), LeaveTime = new TimeSpan(17, 15, 0), EventTime = new TimeSpan(19, 0, 0), RequestedBy = "Coach Chen", AssignedVehicleId = 8, DriverId = 3, StudentsCount = 24 },
                new Activity { ActivityId = 24, ActivityType = "Cultural Exchange", Destination = "International Center", Date = baseDate.AddDays(12), LeaveTime = new TimeSpan(10, 30, 0), EventTime = new TimeSpan(12, 0, 0), RequestedBy = "Ms. Garcia", AssignedVehicleId = 9, DriverId = 4, StudentsCount = 38 },
                new Activity { ActivityId = 25, ActivityType = "Field Trip", Destination = "Space Center", Date = baseDate.AddDays(13), LeaveTime = new TimeSpan(7, 0, 0), EventTime = new TimeSpan(9, 0, 0), RequestedBy = "Mr. Wilson", AssignedVehicleId = 10, DriverId = 5, StudentsCount = 48 },
                new Activity { ActivityId = 26, ActivityType = "Volunteer Work", Destination = "Senior Center", Date = baseDate.AddDays(13), LeaveTime = new TimeSpan(14, 0, 0), EventTime = new TimeSpan(15, 30, 0), RequestedBy = "Mrs. Brown", AssignedVehicleId = 11, DriverId = 6, StudentsCount = 22 },
                new Activity { ActivityId = 27, ActivityType = "Academic Competition", Destination = "Library", Date = baseDate.AddDays(14), LeaveTime = new TimeSpan(9, 45, 0), EventTime = new TimeSpan(11, 15, 0), RequestedBy = "Mr. Martinez", AssignedVehicleId = 12, DriverId = 7, StudentsCount = 14 },
                new Activity { ActivityId = 28, ActivityType = "Sports Event", Destination = "Soccer Complex", Date = baseDate.AddDays(14), LeaveTime = new TimeSpan(16, 0, 0), EventTime = new TimeSpan(18, 30, 0), RequestedBy = "Coach Lee", AssignedVehicleId = 13, DriverId = 8, StudentsCount = 26 },
                new Activity { ActivityId = 29, ActivityType = "Field Trip", Destination = "Observatory", Date = baseDate.AddDays(15), LeaveTime = new TimeSpan(19, 30, 0), EventTime = new TimeSpan(21, 0, 0), RequestedBy = "Dr. Davis", AssignedVehicleId = 14, DriverId = 9, StudentsCount = 20 },
                new Activity { ActivityId = 30, ActivityType = "Graduation Ceremony", Destination = "Auditorium", Date = baseDate.AddDays(15), LeaveTime = new TimeSpan(17, 30, 0), EventTime = new TimeSpan(19, 0, 0), RequestedBy = "Principal Thompson", AssignedVehicleId = 15, DriverId = 10, StudentsCount = 75 }
            );
        }
    }
}
