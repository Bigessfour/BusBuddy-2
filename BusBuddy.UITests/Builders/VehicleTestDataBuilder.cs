using BusBuddy.Core.Models;

namespace BusBuddy.UITests.Builders;

/// <summary>
/// Test data builder for Vehicle objects
/// Provides fluent API for creating test vehicles with realistic data
/// </summary>
public class VehicleTestDataBuilder
{
    private readonly Vehicle _vehicle;

    public VehicleTestDataBuilder()
    {
        _vehicle = new Vehicle
        {
            Id = 1,
            Make = "Blue Bird",
            Model = "Vision",
            PlateNumber = "BUS-001",
            Capacity = 72
        };
    }

    public VehicleTestDataBuilder WithId(int id)
    {
        _vehicle.Id = id;
        return this;
    }

    public VehicleTestDataBuilder WithMake(string make)
    {
        _vehicle.Make = make;
        return this;
    }

    public VehicleTestDataBuilder WithModel(string model)
    {
        _vehicle.Model = model;
        return this;
    }

    public VehicleTestDataBuilder WithPlateNumber(string plateNumber)
    {
        _vehicle.PlateNumber = plateNumber;
        return this;
    }

    public VehicleTestDataBuilder WithCapacity(int capacity)
    {
        _vehicle.Capacity = capacity;
        return this;
    }

    public VehicleTestDataBuilder AsBlueBird()
    {
        _vehicle.Make = "Blue Bird";
        _vehicle.Model = "Vision";
        _vehicle.Capacity = 72;
        return this;
    }

    public VehicleTestDataBuilder AsThomas()
    {
        _vehicle.Make = "Thomas";
        _vehicle.Model = "C2";
        _vehicle.Capacity = 77;
        return this;
    }

    public VehicleTestDataBuilder AsICBus()
    {
        _vehicle.Make = "IC Bus";
        _vehicle.Model = "CE200";
        _vehicle.Capacity = 78;
        return this;
    }

    public Vehicle Build() => _vehicle;

    // Static factory methods for common scenarios
    public static Vehicle CreateStandardBus(int id = 1, string plateNumber = "BUS-001")
    {
        return new VehicleTestDataBuilder()
            .WithId(id)
            .WithPlateNumber(plateNumber)
            .AsBlueBird()
            .Build();
    }

    public static Vehicle CreateLargeBus(int id = 2, string plateNumber = "BUS-002")
    {
        return new VehicleTestDataBuilder()
            .WithId(id)
            .WithPlateNumber(plateNumber)
            .AsICBus()
            .Build();
    }

    public static Vehicle CreateMediumBus(int id = 3, string plateNumber = "BUS-003")
    {
        return new VehicleTestDataBuilder()
            .WithId(id)
            .WithPlateNumber(plateNumber)
            .AsThomas()
            .Build();
    }

    public static List<Vehicle> CreateVehicleFleet(int count = 5)
    {
        var vehicles = new List<Vehicle>();
        var makes = new[] { "Blue Bird", "Thomas", "IC Bus", "Freightliner", "International" };
        var models = new[] { "Vision", "C2", "CE200", "S2C", "3000" };
        var capacities = new[] { 71, 72, 77, 78, 84 };

        for (int i = 1; i <= count; i++)
        {
            var makeIndex = (i - 1) % makes.Length;
            vehicles.Add(new VehicleTestDataBuilder()
                .WithId(i)
                .WithMake(makes[makeIndex])
                .WithModel(models[makeIndex])
                .WithPlateNumber($"BUS-{i:D3}")
                .WithCapacity(capacities[makeIndex])
                .Build());
        }
        return vehicles;
    }
}
