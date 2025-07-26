# BusBuddy UI Testing Variables Map
**Generated**: July 25, 2025
**Purpose**: Comprehensive mapping of testable properties and methods for Phase 2 UI Testing

## Dashboard ViewModel Test Variables

### Properties
- **TotalDrivers** (int) - Count of total drivers in system
- **TotalVehicles** (int) - Count of total vehicles in system
- **TotalActivities** (int) - Count of total activities in system
- **ActiveDrivers** (int) - Count of active drivers only
- **RecentActivities** (ObservableCollection<Activity>) - Collection of recent activities

### Methods
- **LoadDashboardAsync()** - Async method to load dashboard data
- **OnPropertyChanged()** - INotifyPropertyChanged implementation

### Test Scenarios
- Data loading validation
- Property change notifications
- Error handling during data load
- Recent activities collection updates

---

## Drivers ViewModel Test Variables

### Properties (BaseViewModel inherited)
- **IsLoading** (bool) - Loading state indicator
- **ErrorMessage** (string?) - Error message display

### Properties (DriversViewModel specific)
- **SearchText** (string) - Search filter text
- **SelectedDriver** (Driver?) - Currently selected driver
- **Drivers** (ObservableCollection<Driver>) - All drivers collection
- **FilteredDrivers** (ObservableCollection<Driver>) - Filtered drivers collection

### Commands
- **LoadDriversCommand** (ICommand) - Load drivers command
- **RefreshCommand** (ICommand) - Refresh data command
- **ClearSearchCommand** (ICommand) - Clear search filter command
- **EditDriverCommand** (ICommand) - Edit selected driver command
- **DeleteDriverCommand** (ICommand) - Delete selected driver command

### Methods
- **LoadDriversAsync()** - Load drivers from database
- **RefreshDriversAsync()** - Refresh driver data
- **FilterDrivers()** - Apply search filter
- **ClearSearch()** - Clear search filter
- **EditDriver()** - Edit driver action
- **DeleteDriverAsync()** - Delete driver action

---

## Vehicles ViewModel Test Variables

### Properties
- **Vehicles** (ObservableCollection<VehicleRecord>) - All vehicles collection
- **SelectedVehicle** (VehicleRecord?) - Currently selected vehicle

### Commands
- **AddVehicleCommand** (ICommand) - Add new vehicle command
- **EditVehicleCommand** (ICommand) - Edit selected vehicle command
- **DeleteVehicleCommand** (ICommand) - Delete selected vehicle command

### Methods
- **AddVehicle()** - Add new vehicle action
- **EditVehicle()** - Edit vehicle action
- **DeleteVehicle()** - Delete vehicle action
- **LoadVehicleData()** - Load sample vehicle data

---

## Activity Schedule ViewModel Test Variables

### Collections
- **ActivitySchedules** (ObservableCollection<ActivitySchedule>) - All activities
- **FilteredActivitySchedules** (ObservableCollection<ActivitySchedule>) - Filtered activities

### Selection
- **SelectedActivity** (ActivitySchedule?) - Currently selected activity

### Statistics Properties
- **TodayActivitiesCount** (int) - Count of today's activities
- **ScheduledTripsCount** (int) - Count of scheduled trips
- **InProgressCount** (int) - Count of in-progress activities
- **NeedsAttentionCount** (int) - Count of activities needing attention

### Filter Properties
- **StatusFilter** (string) - Status filter value (default: "All")
- **StartDateFilter** (string) - Start date filter (default: today)
- **EndDateFilter** (string) - End date filter (default: today + 30 days)
- **SelectedDate** (DateTime) - Selected calendar date
- **SelectedViewType** (string) - View type (default: "Month")

### Commands
- **RefreshCommand** (ICommand) - Refresh activities command
- **AddActivityCommand** (ICommand) - Add new activity command
- **EditActivityCommand** (ICommand) - Edit selected activity command
- **ViewDetailsCommand** (ICommand) - View activity details command
- **ConfirmActivityCommand** (ICommand) - Confirm activity command
- **StartActivityCommand** (ICommand) - Start activity command
- **CompleteActivityCommand** (ICommand) - Complete activity command
- **CancelActivityCommand** (ICommand) - Cancel activity command
- **ApplyFilterCommand** (ICommand) - Apply filters command
- **GoToTodayCommand** (ICommand) - Go to today command

### Methods
- **LoadActivitySchedulesAsync()** - Load activities from database
- **ApplyFilters()** - Apply current filter settings
- **UpdateStatistics()** - Update statistics properties
- **AddNewActivity()** - Add new activity action
- **EditActivity()** - Edit activity action
- **ViewDetails()** - View activity details action
- **UpdateActivityStatus(string)** - Update activity status

---

## Core Models Test Variables

### Driver Model
- **DriverId** (int) - Primary key
- **DriverName** (string) - Driver's full name
- **DriverPhone** (string?) - Driver's phone number
- **DriverEmail** (string?) - Driver's email address
- **Address** (string?) - Driver's address
- **City** (string?) - Driver's city
- **State** (string?) - Driver's state
- **Zip** (string?) - Driver's zip code
- **DriversLicenceType** (string) - License type
- **TrainingComplete** (bool) - Training completion status
- **Status** (string) - Driver status (default: "Active")

### Vehicle Model
- **Id** (int) - Primary key
- **Make** (string) - Vehicle make
- **Model** (string) - Vehicle model
- **PlateNumber** (string) - License plate number
- **Capacity** (int) - Passenger capacity

### Activity Model
- **ActivityId** (int) - Primary key
- **Date** (DateTime) - Activity date
- **ActivityType** (string) - Type of activity
- **Destination** (string) - Activity destination
- **LeaveTime** (TimeSpan) - Departure time
- **EventTime** (TimeSpan) - Event time
- **RequestedBy** (string) - Person who requested activity
- **AssignedVehicleId** (int) - Assigned vehicle ID
- **DriverId** (int) - Assigned driver ID
- **Status** (string) - Activity status (default: "Scheduled")

### ActivitySchedule Model
- **ActivityScheduleId** (int) - Primary key
- **ScheduledDate** (DateTime) - Scheduled date
- **TripType** (string) - Type of trip
- **ScheduledVehicleId** (int) - Scheduled vehicle ID
- **ScheduledDestination** (string) - Scheduled destination
- **ScheduledLeaveTime** (TimeSpan) - Scheduled leave time
- **ScheduledEventTime** (TimeSpan) - Scheduled event time
- **ScheduledRiders** (int?) - Number of scheduled riders

---

## Test Infrastructure Requirements

### Test Data Builders Needed
1. **DriverTestDataBuilder** - For creating test Driver objects
2. **VehicleTestDataBuilder** - For creating test Vehicle/VehicleRecord objects
3. **ActivityTestDataBuilder** - For creating test Activity objects
4. **ActivityScheduleTestDataBuilder** - For creating test ActivitySchedule objects

### Page Objects Needed
1. **DashboardPage** - Dashboard view interactions
2. **DriversPage** - Drivers view interactions
3. **VehiclesPage** - Vehicles view interactions
4. **ActivitySchedulePage** - Activity schedule view interactions

### UI Test Helpers
1. **SyncfusionTestExtensions** - Extensions for Syncfusion controls
2. **UITestHelpers** - General UI testing utilities
3. **FlaUIExtensions** - Extensions for FlaUI automation

### Test Categories
1. **Unit Tests** - ViewModel logic and data operations
2. **Integration Tests** - Database and service interactions
3. **UI Tests** - End-to-end UI automation tests
4. **Command Tests** - Command execution and state validation
