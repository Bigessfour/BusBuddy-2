using Serilog.Core;
using Serilog.Events;
using System;
using System.Diagnostics;

namespace BusBuddy.WPF.Logging
{
    /// <summary>
    /// Enriches log events with performance-related properties from a Stopwatch
    /// </summary>
    public class PerformanceEnricher : ILogEventEnricher
    {
        private readonly Stopwatch _stopwatch;

        public PerformanceEnricher()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            // Add elapsed time since the enricher was created
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ElapsedMs", _stopwatch.ElapsedMilliseconds));

            // Add elapsed time in ticks for more precise calculations
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ElapsedTicks", _stopwatch.ElapsedTicks));

            // Add timestamp of the log event
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "LoggedAt", DateTime.Now.ToString("o")));
        }
    }
}
