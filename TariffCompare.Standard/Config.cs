using System.Runtime.CompilerServices;
using static System.Configuration.ConfigurationManager;

[assembly: InternalsVisibleTo("TariffCompare.Console")]
namespace TariffCompare.Standard
{
    internal static class Config
    {
        public static string Get(string key)  
        {
            return AppSettings.Get(key);
        }
    }
}
