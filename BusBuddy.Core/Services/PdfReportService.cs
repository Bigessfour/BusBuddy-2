using System.IO;
using BusBuddy.Core.Models;
using Serilog;
using System.Text;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;

namespace BusBuddy.Core.Services
{
    /// <summary>
    /// Service for generating PDF reports using Syncfusion PDF libraries
    /// Provides professional PDF generation for various report types
    /// </summary>
    public class PdfReportService
    {
        private static readonly ILogger Logger = Log.ForContext<PdfReportService>();

        /// <summary>
        /// Generates a professional PDF calendar report for activities within a date range
        /// </summary>
        public byte[] GenerateActivityCalendarReport(List<Activity> activities, DateTime startDate, DateTime endDate)
        {
            try
            {
                Logger.Information("Generating PDF calendar report from {StartDate} to {EndDate}", startDate, endDate);

                // Create a new PDF document
                using var document = new PdfDocument();

                // Add a page to the document
                var page = document.Pages.Add();
                var graphics = page.Graphics;

                // Set up fonts and colors
                var titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 20, PdfFontStyle.Bold);
                var headerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 14, PdfFontStyle.Bold);
                var bodyFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
                var accentColor = new PdfColor(11, 126, 200); // BusBuddy primary color
                var textColor = new PdfColor(33, 37, 41); // Dark text

                var currentY = 50f;
                var pageWidth = page.GetClientSize().Width;

                // Header Section
                var headerBrush = new PdfSolidBrush(accentColor);
                graphics.DrawRectangle(headerBrush, new RectangleF(0, 0, pageWidth, 60));

                graphics.DrawString("Bus Buddy - Activity Calendar Report", titleFont,
                    PdfBrushes.White, new PointF(20, 20));

                graphics.DrawString($"Period: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}",
                    headerFont, PdfBrushes.White, new PointF(20, 40));

                currentY = 80f;

                // Summary Section
                graphics.DrawString("Summary", headerFont, new PdfSolidBrush(textColor),
                    new PointF(20, currentY));
                currentY += 30f;

                var totalActivities = activities.Count;
                var activeDrivers = activities.Where(a => a.DriverId.HasValue).Select(a => a.DriverId).Distinct().Count();
                var activeVehicles = activities.Where(a => a.AssignedVehicleId > 0).Select(a => a.AssignedVehicleId).Distinct().Count();

                graphics.DrawString($"Total Activities: {totalActivities}", bodyFont,
                    new PdfSolidBrush(textColor), new PointF(30, currentY));
                currentY += 15f;

                graphics.DrawString($"Active Drivers: {activeDrivers}", bodyFont,
                    new PdfSolidBrush(textColor), new PointF(30, currentY));
                currentY += 15f;

                graphics.DrawString($"Active Vehicles: {activeVehicles}", bodyFont,
                    new PdfSolidBrush(textColor), new PointF(30, currentY));
                currentY += 30f;

                // Activities Details
                graphics.DrawString("Activities", headerFont, new PdfSolidBrush(textColor),
                    new PointF(20, currentY));
                currentY += 20f;

                foreach (var activity in activities.Take(10)) // Limit for demo
                {
                    graphics.DrawString($"• {activity.Date:MM/dd} - {activity.ActivityType}: {activity.Description}",
                        bodyFont, new PdfSolidBrush(textColor), new PointF(30, currentY));
                    currentY += 15f;
                }

                // Footer
                var footerY = page.GetClientSize().Height - 30f;
                graphics.DrawString($"Generated on: {DateTime.Now:MMM dd, yyyy HH:mm}",
                    bodyFont, new PdfSolidBrush(textColor), new PointF(20, footerY));

                // Save the document to memory stream
                using var stream = new MemoryStream();
                document.Save(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error generating PDF calendar report");
                // Fallback to text-based report if PDF generation fails
                return GenerateTextReport(activities, startDate, endDate, "Calendar Report");
            }
        }

        /// <summary>
        /// Generates a professional PDF report for a single activity
        /// </summary>
        public byte[] GenerateActivityReport(Activity activity)
        {
            try
            {
                Logger.Information("Generating PDF report for activity {ActivityId}", activity.ActivityId);

                // Create a new PDF document
                using var document = new PdfDocument();

                // Add a page to the document
                var page = document.Pages.Add();
                var graphics = page.Graphics;

                // Set up fonts and colors
                var titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 20, PdfFontStyle.Bold);
                var headerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 14, PdfFontStyle.Bold);
                var bodyFont = new PdfStandardFont(PdfFontFamily.Helvetica, 11);
                var labelFont = new PdfStandardFont(PdfFontFamily.Helvetica, 11, PdfFontStyle.Bold);
                var accentColor = new PdfColor(11, 126, 200); // BusBuddy primary color
                var textColor = new PdfColor(33, 37, 41); // Dark text

                var currentY = 50f;
                var pageWidth = page.GetClientSize().Width;

                // Header Section
                var headerBrush = new PdfSolidBrush(accentColor);
                graphics.DrawRectangle(headerBrush, new RectangleF(0, 0, pageWidth, 60));

                graphics.DrawString("Bus Buddy - Activity Report", titleFont,
                    PdfBrushes.White, new PointF(20, 15));

                graphics.DrawString($"Activity ID: {activity.ActivityId}", headerFont,
                    PdfBrushes.White, new PointF(20, 35));

                currentY = 80f;

                // Activity Details Section
                graphics.DrawString("Activity Details", headerFont, new PdfSolidBrush(textColor),
                    new PointF(20, currentY));
                currentY += 30f;

                // Create a structured layout for activity details
                var details = new[]
                {
                    ("Type:", activity.ActivityType ?? "Not specified"),
                    ("Description:", activity.Description ?? "Not specified"),
                    ("Destination:", activity.Destination ?? "Not specified"),
                    ("Date:", activity.Date.ToString("MMMM dd, yyyy (dddd)")),
                    ("Departure Time:", activity.LeaveTime.ToString(@"hh\:mm tt")),
                    ("Return Time:", activity.ReturnTime.ToString(@"hh\:mm tt")),
                    ("Duration:", $"{(activity.ReturnTime - activity.LeaveTime).TotalHours:F1} hours"),
                    ("Driver:", activity.Driver?.FullName ?? "Not assigned"),
                    ("Vehicle:", activity.AssignedVehicle?.BusNumber ?? "Not assigned"),
                    ("Route:", activity.Route?.RouteName ?? "Not assigned"),
                    ("Status:", activity.Status ?? "Not specified")
                };

                foreach (var (label, value) in details)
                {
                    graphics.DrawString(label, labelFont, new PdfSolidBrush(textColor),
                        new PointF(30, currentY));
                    graphics.DrawString(value, bodyFont, new PdfSolidBrush(textColor),
                        new PointF(150, currentY));
                    currentY += 20f;
                }

                // Footer
                var footerY = page.GetClientSize().Height - 30f;
                graphics.DrawString($"Generated on: {DateTime.Now:MMM dd, yyyy HH:mm}",
                    bodyFont, new PdfSolidBrush(textColor), new PointF(20, footerY));

                // Save the document to memory stream
                using var stream = new MemoryStream();
                document.Save(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error generating PDF activity report");
                // Fallback to text-based report if PDF generation fails
                return GenerateTextReport(new List<Activity> { activity }, DateTime.Now, DateTime.Now, "Activity Report");
            }
        }

        #region Fallback Text Generation (Used when PDF generation fails)

        private byte[] GenerateTextReport(List<Activity> activities, DateTime startDate, DateTime endDate, string reportType)
        {
            try
            {
                Logger.Information("Generating fallback text report: {ReportType}", reportType);

                if (reportType == "Calendar Report")
                {
                    var reportContent = GenerateCalendarReportContent(activities, startDate, endDate);
                    return Encoding.UTF8.GetBytes(reportContent);
                }
                else if (activities.Count == 1)
                {
                    var reportContent = GenerateActivityReportContent(activities[0]);
                    return Encoding.UTF8.GetBytes(reportContent);
                }
                else
                {
                    return Encoding.UTF8.GetBytes("Error: Unable to generate report");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error generating fallback text report");
                return Encoding.UTF8.GetBytes($"Error generating report: {ex.Message}");
            }
        }

        private string GenerateCalendarReportContent(List<Activity> activities, DateTime startDate, DateTime endDate)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("BUS BUDDY - ACTIVITY CALENDAR REPORT");
            sb.AppendLine($"Report Period: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}");
            sb.AppendLine($"Generated: {DateTime.Now:MMM dd, yyyy HH:mm}");
            sb.AppendLine(new string('=', 60));
            sb.AppendLine();

            // Summary
            sb.AppendLine("SUMMARY");
            sb.AppendLine($"Total Activities: {activities.Count}");
            sb.AppendLine($"Active Drivers: {activities.Where(a => a.DriverId.HasValue).Select(a => a.DriverId).Distinct().Count()}");
            sb.AppendLine($"Active Vehicles: {activities.Where(a => a.AssignedVehicleId > 0).Select(a => a.AssignedVehicleId).Distinct().Count()}");
            sb.AppendLine();

            // Status breakdown
            var statusGroups = activities.GroupBy(a => a.Status).ToList();
            if (statusGroups.Any())
            {
                sb.AppendLine("STATUS BREAKDOWN");
                foreach (var group in statusGroups.OrderBy(g => g.Key))
                {
                    sb.AppendLine($"  {group.Key}: {group.Count()}");
                }
                sb.AppendLine();
            }

            // Activities by date
            sb.AppendLine("ACTIVITIES BY DATE");
            sb.AppendLine(new string('-', 60));

            var activitiesByDate = activities.GroupBy(a => a.Date.Date).OrderBy(g => g.Key);

            foreach (var dateGroup in activitiesByDate)
            {
                sb.AppendLine($"{dateGroup.Key:dddd, MMMM dd, yyyy}");

                foreach (var activity in dateGroup.OrderBy(a => a.LeaveTime))
                {
                    sb.AppendLine($"  {activity.LeaveTime:HH:mm}-{activity.ReturnTime:HH:mm} | {activity.ActivityType} | {activity.Description}");
                    sb.AppendLine($"    Driver: {activity.Driver?.FullName ?? "Unassigned"}");
                    sb.AppendLine($"    Vehicle: {activity.AssignedVehicle?.BusNumber ?? "Unassigned"}");
                    sb.AppendLine($"    Destination: {activity.Destination}");
                    sb.AppendLine($"    Status: {activity.Status}");
                    sb.AppendLine();
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string GenerateActivityReportContent(Activity activity)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("BUS BUDDY - ACTIVITY REPORT");
            sb.AppendLine($"Activity ID: {activity.ActivityId}");
            sb.AppendLine($"Generated: {DateTime.Now:MMM dd, yyyy HH:mm}");
            sb.AppendLine(new string('=', 50));
            sb.AppendLine();

            // Activity Details
            sb.AppendLine("ACTIVITY DETAILS");
            sb.AppendLine($"Type: {activity.ActivityType ?? "Not specified"}");
            sb.AppendLine($"Description: {activity.Description ?? "Not specified"}");
            sb.AppendLine($"Destination: {activity.Destination ?? "Not specified"}");
            sb.AppendLine($"Date: {activity.Date:dddd, MMMM dd, yyyy}");
            sb.AppendLine($"Departure: {activity.LeaveTime:HH:mm}");
            sb.AppendLine($"Return: {activity.ReturnTime:HH:mm}");
            sb.AppendLine($"Duration: {(activity.ReturnTime - activity.LeaveTime).TotalHours:F1} hours");
            sb.AppendLine();

            // Assignment Details
            sb.AppendLine("ASSIGNMENT DETAILS");
            sb.AppendLine($"Driver: {activity.Driver?.FullName ?? "Not assigned"}");
            sb.AppendLine($"Vehicle: {activity.AssignedVehicle?.BusNumber ?? "Not assigned"}");
            sb.AppendLine($"Route: {activity.Route?.RouteName ?? "Not assigned"}");
            sb.AppendLine($"Requested By: {activity.RequestedBy ?? "Not specified"}");
            sb.AppendLine($"Status: {activity.Status ?? "Not specified"}");
            sb.AppendLine($"Expected Passengers: {activity.ExpectedPassengers?.ToString() ?? "Not specified"}");
            sb.AppendLine();

            // Administrative Details
            sb.AppendLine("ADMINISTRATIVE DETAILS");
            sb.AppendLine($"Created: {activity.CreatedDate:MMM dd, yyyy HH:mm}");
            sb.AppendLine($"Last Updated: {activity.UpdatedDate?.ToString("MMM dd, yyyy HH:mm") ?? "Never"}");

            if (activity.ApprovalDate.HasValue)
            {
                sb.AppendLine($"Approved: {activity.ApprovalDate.Value:MMM dd, yyyy HH:mm}");
                sb.AppendLine($"Approved By: {activity.ApprovedBy ?? "Not specified"}");
            }
            sb.AppendLine();

            // Notes
            if (!string.IsNullOrEmpty(activity.Notes))
            {
                sb.AppendLine("NOTES");
                sb.AppendLine(activity.Notes);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        #endregion
    }
}
