using static System.Configuration.ConfigurationManager;

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
