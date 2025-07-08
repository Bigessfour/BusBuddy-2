
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.UI.Xaml.Scheduler;

namespace Bus_Buddy.Utilities
{
    /// <summary>
    /// Syncfusion Scheduler Manager for WPF (SfScheduler)
    /// Provides scheduling functionality for Bus Buddy application using Syncfusion WPF controls
    /// </summary>
    public class SyncfusionSchedulerManagerWpf
    {
        public ObservableCollection<SchedulerAppointment> Appointments { get; } = new ObservableCollection<SchedulerAppointment>();

        public SyncfusionSchedulerManagerWpf()
        {
        }

        public void LoadAppointments(IEnumerable<SchedulerAppointment> appointments)
        {
            Appointments.Clear();
            foreach (var appt in appointments)
                Appointments.Add(appt);
        }

        public void AddAppointment(SchedulerAppointment appointment)
        {
            Appointments.Add(appointment);
        }

        public void RemoveAppointment(SchedulerAppointment appointment)
        {
            Appointments.Remove(appointment);
        }

        public void UpdateAppointment(SchedulerAppointment oldAppointment, SchedulerAppointment newAppointment)
        {
            var idx = Appointments.IndexOf(oldAppointment);
            if (idx >= 0)
                Appointments[idx] = newAppointment;
        }

        public IEnumerable<SchedulerAppointment> GetAppointmentsForDate(DateTime date)
        {
            return Appointments.Where(a => a.StartTime.Date == date.Date);
        }

        public IEnumerable<SchedulerAppointment> GetAppointmentsForDateRange(DateTime start, DateTime end)
        {
            return Appointments.Where(a => a.StartTime.Date >= start.Date && a.EndTime.Date <= end.Date);
        }
    }
}
