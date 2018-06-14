using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;

[assembly: InternalsVisibleTo("TariffCompare.Console")]
namespace TariffCompare.Standard
{
    internal class Datasource : AbstractDatasource
    {
        public override (string tariff, (float power, float gas) rates, float standingCharge)[] Tariffs { get; }

        public Datasource(string path)
        {
            string fullPath = Path.GetFullPath(path);
            string file = File.ReadAllLines(fullPath).Aggregate("", (s, a) => s + a);
            Tariffs = ParseJson(file);
        }

    }

    internal abstract class AbstractDatasource : IDatasource
    {
        private const string TARIFF = "tariff";
        private const string RATES = "rates";
        private const string POWER = "power";
        private const string GAS = "gas";
        private const string STANDING_CHARGE = "standing_charge";

        public abstract (string tariff, (float power, float gas) rates, float standingCharge)[] Tariffs { get; }

        protected (string tariff, (float power, float gas) rates, float standingCharge)[] ParseJson(string json)
        {
            var temp = new List<(string, (float, float), float)>();
            JArray jArray = JArray.Parse(json);
            foreach (JToken jToken in jArray)
            {
                if (jToken.HasValues && jToken[TARIFF] != null)
                {
                    string tariff = jToken[TARIFF].ToString();
                    float power = float.Parse(jToken[RATES][POWER]?.ToString() ?? "0");
                    float gas = float.Parse(jToken[RATES][GAS]?.ToString() ?? "0");
                    float standingCharge = float.Parse(jToken[STANDING_CHARGE].ToString());
                    temp.Add((tariff, (power, gas), standingCharge));
                }
            }
            return temp.ToArray();
        }
    }

    public interface IDatasource
    {
        (string tariff, (float power, float gas) rates, float standingCharge)[] Tariffs { get; }
    }
}
