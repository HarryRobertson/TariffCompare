using System.Configuration;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TariffCompare.Linux")]
namespace TariffCompare.Standard
{
    internal static class Config
    {
        public static string Get(string key)  
        {
            return ConfigurationManager.AppSettings.Get(key);
        }
    }
}
