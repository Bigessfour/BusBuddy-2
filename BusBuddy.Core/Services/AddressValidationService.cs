using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BusBuddy.Core.Data.UnitOfWork;
using BusBuddy.Core.Models;

namespace BusBuddy.Core.Services
{
    /// <summary>
    /// Implementation of address validation service that uses a combination of
    /// basic regex validation and the database of known bus stops
    /// In a production environment, this would likely call an external geocoding API
    /// </summary>
    public class AddressValidationService : IAddressValidationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddressValidationService>? _logger;

        public AddressValidationService(IUnitOfWork unitOfWork, ILogger<AddressValidationService>? logger = null)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger;
        }

        /// <inheritdoc />
        public Task<(bool IsValid, string? NormalizedAddress)> ValidateAddressAsync(
            string address, string? city = null, string? state = null, string? zip = null)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return Task.FromResult<(bool IsValid, string? NormalizedAddress)>((false, null));
            }

            try
            {
                // Basic address validation using regex patterns
                // In a real implementation, this would use a geocoding service
                var isValidStreet = Regex.IsMatch(address, @"^\d+\s+[\w\s]+$");
                var isValidCity = string.IsNullOrWhiteSpace(city) || Regex.IsMatch(city, @"^[A-Za-z\s]+$");
                var isValidState = string.IsNullOrWhiteSpace(state) || Regex.IsMatch(state, @"^[A-Z]{2}$");
                var isValidZip = string.IsNullOrWhiteSpace(zip) || Regex.IsMatch(zip, @"^\d{5}(-\d{4})?$");

                bool isValid = isValidStreet && isValidCity && isValidState && isValidZip;

                if (isValid)
                {
                    // Normalize the address (capitalize first letters, standardize formatting)
                    var normalizedAddress = NormalizeAddress(address, city, state, zip);
                    return Task.FromResult<(bool IsValid, string? NormalizedAddress)>((true, normalizedAddress));
                }

                return Task.FromResult<(bool IsValid, string? NormalizedAddress)>((false, null));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating address: {Address}", address);
                return Task.FromResult((false, (string?)null));
            }
        }

        /// <inheritdoc />
        public async Task<List<string>> FindNearbyBusStopsAsync(
            string address, string? city = null, string? state = null, string? zip = null)
        {
            try
            {
                // In a real implementation, this would query a geocoding/routing service
                // For this implementation, we'll fetch all routes from the database
                // and create mock bus stops based on the routes

                // Get all routes from the database
                var routes = await _unitOfWork.Routes.GetAllAsync();

                // Extract all unique bus stops from routes
                // Since our Route model doesn't have explicit bus stops, we'll create simulated ones
                var allBusStops = new HashSet<string>();

                // For each route, create simulated bus stops based on route name
                foreach (var route in routes)
                {
                    // Create 3 stops for each route
                    allBusStops.Add($"{route.RouteName} - Start");
                    allBusStops.Add($"{route.RouteName} - Middle");
                    allBusStops.Add($"{route.RouteName} - End");

                    // Add some named stops if we have a description
                    if (!string.IsNullOrEmpty(route.Description))
                    {
                        // Use the description to create a stop name
                        allBusStops.Add($"{route.Description} Stop");
                    }
                }

                // For simplicity, just return a subset of stops based on the address
                // In a real implementation, this would use geospatial calculations
                var result = new List<string>();

                // Extract street number from address for naive matching
                var streetNumber = 0;
                var match = Regex.Match(address, @"^(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out streetNumber))
                {
                    // Use street number to "find" nearby stops - purely for simulation
                    int bucketSize = 1000; // Group addresses into buckets of 1000
                    int bucket = streetNumber / bucketSize;

                    // Take a few stops from all stops based on the "bucket"
                    result = allBusStops
                        .OrderBy(s => s)
                        .Skip(bucket % Math.Max(1, allBusStops.Count - 5))
                        .Take(5)
                        .ToList();
                }

                // If we couldn't find stops based on address, just return some defaults
                if (result.Count == 0 && allBusStops.Count > 0)
                {
                    result = allBusStops.Take(Math.Min(5, allBusStops.Count)).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error finding nearby bus stops for address: {Address}", address);
                return new List<string>();
            }
        }

        /// <inheritdoc />
        public Task<double> GetDistanceToBusStopAsync(string address, string busStop)
        {
            try
            {
                // In a real implementation, this would use a mapping/routing service
                // For this demo, we'll generate a random distance between 0.1 and 2.0 miles
                var random = new Random();
                return Task.FromResult(Math.Round(0.1 + random.NextDouble() * 1.9, 2));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating distance from {Address} to {BusStop}", address, busStop);
                return Task.FromResult(-1.0); // Indicates error
            }
        }

        /// <inheritdoc />
        public string FormatAddress(string address, string? city = null, string? state = null, string? zip = null)
        {
            // Combine all non-null components
            var parts = new List<string> { address.Trim() };

            if (!string.IsNullOrWhiteSpace(city))
            {
                if (!string.IsNullOrWhiteSpace(state))
                {
                    // City and state together: "City, ST"
                    parts.Add($"{city.Trim()}, {state.Trim().ToUpperInvariant()}");
                }
                else
                {
                    // Just city
                    parts.Add(city.Trim());
                }
            }
            else if (!string.IsNullOrWhiteSpace(state))
            {
                // Just state
                parts.Add(state.Trim().ToUpperInvariant());
            }

            // Add zip if provided
            if (!string.IsNullOrWhiteSpace(zip))
            {
                parts.Add(zip.Trim());
            }

            // Join with appropriate spacing
            return string.Join(" ", parts);
        }

        /// <inheritdoc />
        public (string Address, string? City, string? State, string? Zip) ParseAddress(string formattedAddress)
        {
            if (string.IsNullOrWhiteSpace(formattedAddress))
            {
                return (string.Empty, null, null, null);
            }

            try
            {
                // This is a simplistic parser - a real implementation would be more robust
                var parts = formattedAddress.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Extract ZIP (last element if it's all digits or has a hyphen for ZIP+4)
                string? zip = null;
                if (parts.Length > 0 && Regex.IsMatch(parts[parts.Length - 1], @"^\d{5}(-\d{4})?$"))
                {
                    zip = parts[parts.Length - 1];
                    parts = parts.Take(parts.Length - 1).ToArray();
                }

                // Extract state (second-to-last element if it's 2 uppercase letters)
                string? state = null;
                if (parts.Length > 0 && Regex.IsMatch(parts[parts.Length - 1], @"^[A-Z]{2}$"))
                {
                    state = parts[parts.Length - 1];
                    parts = parts.Take(parts.Length - 1).ToArray();
                }

                // Extract city (everything before the state that follows a comma)
                string? city = null;
                var addressParts = string.Join(" ", parts).Split(new[] { ',' }, 2);
                if (addressParts.Length > 1)
                {
                    city = addressParts[1].Trim();
                    parts = addressParts[0].Split(' ');
                }

                // The rest is the street address
                string address = string.Join(" ", parts);

                return (address, city, state, zip);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error parsing address: {FormattedAddress}", formattedAddress);
                return (formattedAddress, null, null, null);
            }
        }

        private string NormalizeAddress(string address, string? city = null, string? state = null, string? zip = null)
        {
            // Basic normalization - capitalize first letters of words, trim extra spaces
            address = string.Join(" ", address.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant()));

            // Normalize city if provided
            if (!string.IsNullOrWhiteSpace(city))
            {
                city = string.Join(" ", city.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant()));
            }

            // Normalize state if provided (ensure uppercase)
            if (!string.IsNullOrWhiteSpace(state))
            {
                state = state.ToUpperInvariant();
            }

            // Return formatted address
            return FormatAddress(address, city, state, zip);
        }
    }
}
