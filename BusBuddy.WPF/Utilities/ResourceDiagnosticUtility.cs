using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Utility class for diagnosing resource loading issues
    /// </summary>
    public static class ResourceDiagnosticUtility
    {
        /// <summary>
        /// Logs all embedded resources found in the specified assembly
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to inspect</param>
        /// <param name="logger">Logger instance for output</param>
        public static void LogAllEmbeddedResources(string assemblyName, ILogger logger)
        {
            try
            {
                // Try to load the assembly
                Assembly? assembly = null;
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch
                {
                    // If we can't load by name, try to find it among loaded assemblies
                    foreach (var loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (loadedAssembly.GetName().Name == assemblyName)
                        {
                            assembly = loadedAssembly;
                            break;
                        }
                    }
                }

                if (assembly == null)
                {
                    logger.LogWarning($"[RESOURCE_DIAGNOSTIC] Could not load assembly: {assemblyName}");
                    return;
                }

                // Get all embedded resources
                string[] resources = assembly.GetManifestResourceNames();
                logger.LogInformation($"[RESOURCE_DIAGNOSTIC] Found {resources.Length} resources in {assemblyName}:");

                StringBuilder sb = new StringBuilder();
                foreach (string resource in resources)
                {
                    sb.AppendLine($"  • {resource}");
                }

                logger.LogInformation($"[RESOURCE_DIAGNOSTIC] Resources in {assemblyName}:\n{sb}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"[RESOURCE_DIAGNOSTIC] Error inspecting resources in {assemblyName}");
            }
        }

        /// <summary>
        /// Attempts to load a resource from an assembly and save it to a file for inspection
        /// </summary>
        /// <param name="assemblyName">The name of the assembly containing the resource</param>
        /// <param name="resourceName">The name of the resource to extract</param>
        /// <param name="outputPath">The path where the resource should be saved</param>
        /// <param name="logger">Logger instance for output</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool ExtractResourceToFile(string assemblyName, string resourceName, string outputPath, ILogger logger)
        {
            try
            {
                Assembly? assembly = Assembly.Load(assemblyName);
                if (assembly == null)
                {
                    logger.LogWarning($"[RESOURCE_DIAGNOSTIC] Could not load assembly: {assemblyName}");
                    return false;
                }

                using (Stream? resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (resourceStream == null)
                    {
                        logger.LogWarning($"[RESOURCE_DIAGNOSTIC] Resource not found: {resourceName}");
                        return false;
                    }

                    using (FileStream fileStream = new FileStream(outputPath, FileMode.Create))
                    {
                        resourceStream.CopyTo(fileStream);
                    }
                }

                logger.LogInformation($"[RESOURCE_DIAGNOSTIC] Resource extracted to: {outputPath}");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"[RESOURCE_DIAGNOSTIC] Error extracting resource {resourceName} from {assemblyName}");
                return false;
            }
        }

        /// <summary>
        /// Logs information about all loaded assemblies in the current AppDomain
        /// </summary>
        /// <param name="logger">Logger instance for output</param>
        public static void LogLoadedAssemblies(ILogger logger)
        {
            try
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                logger.LogInformation($"[RESOURCE_DIAGNOSTIC] Found {assemblies.Length} loaded assemblies:");

                var syncfusionAssemblies = new List<Assembly>();
                StringBuilder sb = new StringBuilder();

                foreach (Assembly assembly in assemblies)
                {
                    string? name = assembly.GetName().Name;
                    string version = assembly.GetName().Version?.ToString() ?? "Unknown";
                    string location = string.IsNullOrEmpty(assembly.Location) ? "[Dynamic Assembly]" : assembly.Location;

                    if (name != null && name.StartsWith("Syncfusion."))
                    {
                        syncfusionAssemblies.Add(assembly);
                        sb.AppendLine($"  • {name} ({version}) - {location}");
                    }
                }

                logger.LogInformation($"[RESOURCE_DIAGNOSTIC] Syncfusion assemblies ({syncfusionAssemblies.Count}):\n{sb}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[RESOURCE_DIAGNOSTIC] Error inspecting loaded assemblies");
            }
        }
    }
}
