using BusBuddy.Core.Models;

namespace BusBuddy.UITests.Builders;

/// <summary>
/// Test data builder for Driver objects
/// Provides fluent API for creating test drivers with realistic data
/// </summary>
public class DriverTestDataBuilder
{
    private readonly Driver _driver;

    public DriverTestDataBuilder()
    {
        _driver = new Driver
        {
            DriverId = 1,
            DriverName = "Test Driver",
            DriverPhone = "(555) 123-4567",
            DriverEmail = "test.driver@busbuddy.com",
            Address = "123 Test Street",
            City = "Test City",
            State = "TS",
            Zip = "12345",
            DriversLicenceType = "CDL Class B",
            TrainingComplete = true,
            Status = "Active"
        };
    }

    public DriverTestDataBuilder WithId(int id)
    {
        _driver.DriverId = id;
        return this;
    }

    public DriverTestDataBuilder WithName(string name)
    {
        _driver.DriverName = name;
        return this;
    }

    public DriverTestDataBuilder WithPhone(string phone)
    {
        _driver.DriverPhone = phone;
        return this;
    }

    public DriverTestDataBuilder WithEmail(string email)
    {
        _driver.DriverEmail = email;
        return this;
    }

    public DriverTestDataBuilder WithAddress(string address, string city, string state, string zip)
    {
        _driver.Address = address;
        _driver.City = city;
        _driver.State = state;
        _driver.Zip = zip;
        return this;
    }

    public DriverTestDataBuilder WithLicenseType(string licenseType)
    {
        _driver.DriversLicenceType = licenseType;
        return this;
    }

    public DriverTestDataBuilder WithTrainingComplete(bool isComplete)
    {
        _driver.TrainingComplete = isComplete;
        return this;
    }

    public DriverTestDataBuilder WithStatus(string status)
    {
        _driver.Status = status;
        return this;
    }

    public DriverTestDataBuilder AsInactive()
    {
        _driver.Status = "Inactive";
        return this;
    }

    public DriverTestDataBuilder AsActive()
    {
        _driver.Status = "Active";
        return this;
    }

    public DriverTestDataBuilder WithoutTraining()
    {
        _driver.TrainingComplete = false;
        return this;
    }

    public Driver Build() => _driver;

    // Static factory methods for common scenarios
    public static Driver CreateActiveDriver(int id = 1, string name = "Active Driver")
    {
        return new DriverTestDataBuilder()
            .WithId(id)
            .WithName(name)
            .AsActive()
            .WithTrainingComplete(true)
            .Build();
    }

    public static Driver CreateInactiveDriver(int id = 2, string name = "Inactive Driver")
    {
        return new DriverTestDataBuilder()
            .WithId(id)
            .WithName(name)
            .AsInactive()
            .Build();
    }

    public static Driver CreateTraineeDriver(int id = 3, string name = "Trainee Driver")
    {
        return new DriverTestDataBuilder()
            .WithId(id)
            .WithName(name)
            .WithoutTraining()
            .WithStatus("Training")
            .Build();
    }

    public static List<Driver> CreateDriversList(int count = 5)
    {
        var drivers = new List<Driver>();
        for (int i = 1; i <= count; i++)
        {
            drivers.Add(new DriverTestDataBuilder()
                .WithId(i)
                .WithName($"Test Driver {i}")
                .WithEmail($"driver{i}@busbuddy.com")
                .WithPhone($"(555) 123-456{i}")
                .Build());
        }
        return drivers;
    }
}
