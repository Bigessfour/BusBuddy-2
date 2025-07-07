using Microsoft.Extensions.Configuration;

namespace BusBuddy.Core.Services
{
    public interface IConfigurationService
    {
        string GetConnectionString(string name);
        T GetValue<T>(string key);
        string GetSyncfusionLicenseKey();
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString(string name)
        {
            return _configuration.GetConnectionString(name) ?? string.Empty;
        }

        public T GetValue<T>(string key)
        {
            return _configuration.GetValue<T>(key) ?? default(T)!;
        }

        public string GetSyncfusionLicenseKey()
        {
            return _configuration["SyncfusionLicenseKey"] ?? "Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXhec3RSRGRYU0R2WUBWYEk=";
        }
    }
}
