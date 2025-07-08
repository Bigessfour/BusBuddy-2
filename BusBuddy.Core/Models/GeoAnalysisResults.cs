namespace BusBuddy.Core.Models
{
    public class TerrainAnalysisResult
    {
        public double MinElevation { get; set; }
        public double MaxElevation { get; set; }
        public double AverageSlope { get; set; }
        public string TerrainType { get; set; } = string.Empty;
        public string RouteDifficulty { get; set; } = string.Empty;
    }

    public class WeatherData
    {
        public string Condition { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public double Visibility { get; set; }
        public string WindCondition { get; set; } = string.Empty;
    }

    public class TrafficData
    {
        public string OverallCondition { get; set; } = string.Empty;
        public Dictionary<string, string> RouteConditions { get; set; } = new();
    }
}
