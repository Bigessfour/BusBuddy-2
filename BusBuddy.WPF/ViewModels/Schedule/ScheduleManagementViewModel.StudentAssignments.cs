using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Core.Models;
using Serilog;

namespace BusBuddy.WPF.ViewModels.Schedule
{
    /// <summary>
    /// Partial class containing student assignment functionality for ScheduleManagementViewModel
    /// </summary>
    public partial class ScheduleManagementViewModel
    {
        /// <summary>
        /// View student assignments for the selected schedule
        /// </summary>
        private async Task ViewStudentAssignmentsAsync()
        {
            if (SelectedSchedule == null) return;

            try
            {
                IsBusy = true;
                Logger.Information("Loading student assignments for schedule {ScheduleId}", SelectedSchedule.ScheduleId);

                var students = await _studentScheduleService.GetStudentsForScheduleAsync(SelectedSchedule.ScheduleId);
                AssignedStudents.Clear();
                foreach (var student in students)
                {
                    AssignedStudents.Add(student);
                }

                var studentSchedules = await _studentScheduleService.GetSchedulesForStudentAsync(SelectedSchedule.ScheduleId);
                StudentSchedules.Clear();
                foreach (var studentSchedule in studentSchedules)
                {
                    StudentSchedules.Add(studentSchedule);
                }

                AssignedStudentsCount = AssignedStudents.Count;

                Logger.Information("Loaded {Count} student assignments for schedule {ScheduleId}",
                    AssignedStudents.Count, SelectedSchedule.ScheduleId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading student assignments for schedule {ScheduleId}", SelectedSchedule?.ScheduleId);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Open the student assignment management dialog
        /// </summary>
        private async Task ManageStudentAssignmentsAsync()
        {
            if (SelectedSchedule == null) return;

            try
            {
                IsBusy = true;
                Logger.Information("Opening student assignment management for schedule {ScheduleId}", SelectedSchedule.ScheduleId);

                // This would open a dialog for managing student assignments
                // For now, we'll just log the action
                await Task.Delay(100); // Simulate async operation

                Logger.Information("Student assignment management completed for schedule {ScheduleId}", SelectedSchedule.ScheduleId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error managing student assignments for schedule {ScheduleId}", SelectedSchedule?.ScheduleId);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Assign multiple students to the selected schedule
        /// </summary>
        private async Task AssignStudentsToScheduleAsync()
        {
            if (SelectedSchedule == null) return;

            try
            {
                IsBusy = true;
                Logger.Information("Assigning students to schedule {ScheduleId}", SelectedSchedule.ScheduleId);

                // This would open a dialog to select students for assignment
                // For now, we'll just log the action
                await Task.Delay(100); // Simulate async operation

                // Refresh the student assignments after assignment
                await ViewStudentAssignmentsAsync();

                Logger.Information("Students assigned to schedule {ScheduleId}", SelectedSchedule.ScheduleId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error assigning students to schedule {ScheduleId}", SelectedSchedule?.ScheduleId);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Remove a student from the selected schedule
        /// </summary>
        private async Task RemoveStudentFromScheduleAsync()
        {
            if (SelectedSchedule == null) return;

            try
            {
                IsBusy = true;
                Logger.Information("Removing student from schedule {ScheduleId}", SelectedSchedule.ScheduleId);

                // This would open a dialog to select student for removal
                // For now, we'll just log the action
                await Task.Delay(100); // Simulate async operation

                // Refresh the student assignments after removal
                await ViewStudentAssignmentsAsync();

                Logger.Information("Student removed from schedule {ScheduleId}", SelectedSchedule.ScheduleId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error removing student from schedule {ScheduleId}", SelectedSchedule?.ScheduleId);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Load assigned students for the selected schedule
        /// Called when schedule selection changes
        /// </summary>
        private async Task LoadScheduleStudentsAsync()
        {
            if (SelectedSchedule == null)
            {
                AssignedStudents.Clear();
                StudentSchedules.Clear();
                AssignedStudentsCount = 0;
                return;
            }

            try
            {
                Logger.Information("Loading students for schedule {ScheduleId}", SelectedSchedule.ScheduleId);

                var students = await _studentScheduleService.GetStudentsForScheduleAsync(SelectedSchedule.ScheduleId);
                AssignedStudents.Clear();
                foreach (var student in students)
                {
                    AssignedStudents.Add(student);
                }

                AssignedStudentsCount = AssignedStudents.Count;

                Logger.Information("Loaded {Count} students for schedule {ScheduleId}",
                    AssignedStudents.Count, SelectedSchedule.ScheduleId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading students for schedule {ScheduleId}", SelectedSchedule?.ScheduleId);
            }
        }

        /// <summary>
        /// Get student assignment statistics for the dashboard
        /// </summary>
        public async Task<Dictionary<string, int>> GetStudentAssignmentStatisticsAsync()
        {
            try
            {
                Logger.Information("Getting student assignment statistics");

                var stats = await _studentScheduleService.GetStudentAssignmentStatisticsAsync();

                Logger.Information("Retrieved student assignment statistics");
                return stats;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error getting student assignment statistics");
                return new Dictionary<string, int>();
            }
        }
    }
}
