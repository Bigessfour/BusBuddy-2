using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BusBuddy.Core.Models;
using BusBuddy.WPF.Models;
using BusBuddy.WPF.Views.Student;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BusBuddy.WPF.ViewModels.Student
{
    public partial class AdvancedSearchViewModel : ObservableObject
    {
        private readonly Window _dialog;

        public AdvancedSearchViewModel(Window dialog, IEnumerable<Route> routes, IEnumerable<BusBuddy.Core.Models.Student> students)
        {
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));

            // Initialize route collection
            if (routes != null)
            {
                var routesList = new List<Route>(routes);
                routesList.Insert(0, new Route { RouteName = "-- Any --" });
                AllRoutes = new ObservableCollection<Route>(routesList);
            }
            else
            {
                AllRoutes = new ObservableCollection<Route> { new Route { RouteName = "-- Any --" } };
            }

            // Initialize grades and schools from existing students
            if (students != null && students.Any())
            {
                var grades = students
                    .Where(s => !string.IsNullOrEmpty(s.Grade))
                    .Select(s => s.Grade ?? string.Empty)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();
                grades.Insert(0, "-- Any --");
                AllGrades = new ObservableCollection<string>(grades);

                var schools = students
                    .Where(s => !string.IsNullOrEmpty(s.School))
                    .Select(s => s.School ?? string.Empty)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();
                schools.Insert(0, "-- Any --");
                AllSchools = new ObservableCollection<string>(schools);
            }
            else
            {
                AllGrades = new ObservableCollection<string> { "-- Any --" };
                AllSchools = new ObservableCollection<string> { "-- Any --" };
            }

            // Set default values
            StudentName = string.Empty;
            StudentNumber = string.Empty;
            Grade = "-- Any --";
            School = "-- Any --";
            ActiveStatus = FilterStatus.All;
            SpecialNeedsStatus = FilterStatus.All;
            AmRoute = "-- Any --";
            PmRoute = "-- Any --";
            BusStop = string.Empty;
            RouteAssignmentStatus = FilterStatus.All;
            City = string.Empty;
            Zip = string.Empty;

            // Initialize empty search criteria
            SearchCriteria = new SearchCriteria
            {
                StudentName = string.Empty,
                StudentNumber = string.Empty,
                Grade = null,
                School = null,
                ActiveStatus = FilterStatus.All,
                SpecialNeedsStatus = FilterStatus.All,
                AMRoute = null,
                PMRoute = null,
                BusStop = string.Empty,
                RouteAssignmentStatus = FilterStatus.All,
                City = string.Empty,
                Zip = string.Empty
            };
        }

        #region Properties

        // Collections for dropdowns
        public ObservableCollection<Route> AllRoutes { get; }
        public ObservableCollection<string> AllGrades { get; }
        public ObservableCollection<string> AllSchools { get; }

        // Filter criteria properties
        [ObservableProperty]
        private string _studentName = string.Empty;

        [ObservableProperty]
        private string _studentNumber = string.Empty;

        [ObservableProperty]
        private string _grade = string.Empty;

        [ObservableProperty]
        private string _school = string.Empty;

        [ObservableProperty]
        private FilterStatus _activeStatus;

        [ObservableProperty]
        private FilterStatus _specialNeedsStatus;

        [ObservableProperty] private string _amRoute = string.Empty;

        [ObservableProperty]
        private string _pmRoute = string.Empty;

        [ObservableProperty]
        private string _busStop = string.Empty;

        [ObservableProperty]
        private FilterStatus _routeAssignmentStatus;

        [ObservableProperty]
        private string _city = string.Empty;

        [ObservableProperty]
        private string _zip = string.Empty;

        // Dialog result properties
        public bool DialogResult { get; private set; }
        public SearchCriteria SearchCriteria { get; private set; }

        #endregion

        #region Commands

        [RelayCommand]
        private void Search()
        {
            SearchCriteria = new SearchCriteria
            {
                StudentName = StudentName,
                StudentNumber = StudentNumber,
                Grade = Grade == "-- Any --" ? null : Grade,
                School = School == "-- Any --" ? null : School,
                ActiveStatus = ActiveStatus,
                SpecialNeedsStatus = SpecialNeedsStatus,
                AMRoute = AmRoute == "-- Any --" ? null : AmRoute,
                PMRoute = PmRoute == "-- Any --" ? null : PmRoute,
                BusStop = BusStop,
                RouteAssignmentStatus = RouteAssignmentStatus,
                City = City,
                Zip = Zip
            };

            DialogResult = true;
            _dialog.DialogResult = true;
            _dialog.Close();
        }

        [RelayCommand]
        private void Clear()
        {
            StudentName = string.Empty;
            StudentNumber = string.Empty;
            Grade = "-- Any --";
            School = "-- Any --";
            ActiveStatus = FilterStatus.All;
            SpecialNeedsStatus = FilterStatus.All;
            AmRoute = "-- Any --";
            PmRoute = "-- Any --";
            BusStop = string.Empty;
            RouteAssignmentStatus = FilterStatus.All;
            City = string.Empty;
            Zip = string.Empty;
        }

        [RelayCommand]
        private void Cancel()
        {
            DialogResult = false;
            _dialog.DialogResult = false;
            _dialog.Close();
        }

        #endregion
    }
}
