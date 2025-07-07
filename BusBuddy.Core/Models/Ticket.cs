using BusBuddy.Core.Models.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusBuddy.Core.Models
{
    public class Ticket : BaseEntity
    {
        [Key]
        public int TicketId { get; set; }

        [Required]
        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }

        [Required]
        [ForeignKey(nameof(Route))]
        public int RouteId { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime TravelDate { get; set; }

        [Required]
        public DateTime IssuedDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string TicketType { get; set; } = string.Empty; // Single, Round Trip, Daily Pass, Weekly Pass, Monthly Pass

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Valid"; // Valid, Used, Expired, Cancelled

        [Required]
        [StringLength(30)]
        public string PaymentMethod { get; set; } = string.Empty; // Cash, Card, Mobile Pay, Student Account

        [StringLength(50)]
        public string QRCode { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }

        public bool IsRefundable { get; set; } = true;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? RefundAmount { get; set; }

        public DateTime? UsedDate { get; set; }

        [StringLength(100)]
        public string? UsedByDriver { get; set; }

        // Navigation properties
        public virtual Student? Student { get; set; }
        public virtual Route? Route { get; set; }

        // Calculated properties
        [NotMapped]
        public bool IsExpired => Status == "Expired" || (ValidUntil.HasValue && ValidUntil.Value < DateTime.Now);

        [NotMapped]
        public bool CanBeUsed => Status == "Valid" && !IsExpired;

        [NotMapped]
        public string DisplayName => $"Ticket #{TicketId} - {Student?.FullName} ({TravelDate:MM/dd/yyyy})";

        [NotMapped]
        public int DaysUntilExpiry
        {
            get
            {
                if (!ValidUntil.HasValue) return int.MaxValue;
                return Math.Max(0, (ValidUntil.Value.Date - DateTime.Today).Days);
            }
        }

        public void MarkAsUsed(string? driverName = null)
        {
            Status = "Used";
            UsedDate = DateTime.Now;
            UsedByDriver = driverName;
        }

        public void CancelTicket()
        {
            Status = "Cancelled";
            if (IsRefundable)
            {
                RefundAmount = Price;
            }
        }

        public void SetValidityPeriod()
        {
            ValidFrom = TravelDate.Date;

            ValidUntil = TicketType switch
            {
                "Single" => TravelDate.Date.AddDays(1), // Valid for 1 day
                "Round Trip" => TravelDate.Date.AddDays(1), // Valid for 1 day
                "Daily Pass" => TravelDate.Date.AddDays(1), // Valid for 1 day
                "Weekly Pass" => TravelDate.Date.AddDays(7), // Valid for 7 days
                "Monthly Pass" => TravelDate.Date.AddDays(30), // Valid for 30 days
                _ => TravelDate.Date.AddDays(1)
            };
        }

        public void GenerateQRCode()
        {
            if (string.IsNullOrEmpty(QRCode))
            {
                var random = new Random();
                QRCode = $"BT{TicketId:0000}{TravelDate:yyMMdd}{random.Next(1000, 9999)}";
            }
        }
    }
}
