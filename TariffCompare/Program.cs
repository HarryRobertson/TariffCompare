using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace TariffCompare
{
    class Program
    {
        static void Main(params string[] args)
        {
            string path = ConfigurationManager.AppSettings.Get("Datasource");
            Datasource ds = new Datasource(path);

            StringBuilder sb = new StringBuilder();
            switch (args[0].ToLower())
            {
                case "cost":
                    if (args.Length != 3 // guard against incorrect arguments
                            || !int.TryParse(args[1], out int powerUsage)
                            || !int.TryParse(args[2], out int gasUsage))
                        sb = GetHelp(true, false);
                    else
                    {
                        var costs = EvaluateCost(ds, powerUsage, gasUsage);
                        foreach (var cost in costs)
                        {
                            sb.AppendLine($"{cost.tariffName} {cost.cost:0.00}");
                        }
                    }
                    break;

                case "usage":
                    if (args.Length != 4 // guard against incorrect arguments
                            || !Enum.TryParse(args[2], true, out Constants.FuelType fuelType)
                            || !int.TryParse(args[3], out int targetMonthlySpend))
                        sb = GetHelp(false, true);
                    else
                    {
                        bool.TryParse(ConfigurationManager.AppSettings.Get("targetMonthlySpend_includesStandingCharge"), out bool includesStandingCharge);
                        float usage = EvaluateUsage(ds, args[1], fuelType, targetMonthlySpend);
                        sb.AppendLine($"{usage:0.00}");
                    }
                    break;

                default: // guard against incorrect arguments
                    sb = GetHelp();
                    break;
            }

            Console.Write(sb); // only toplevel function ever writes to stdout
#if DEBUG
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(); // just to keep the window open during debug
#endif
        }

        private static StringBuilder GetHelp(bool showCost = true, bool showUsage = true)
        {
            if (!showCost && !showUsage) throw new System.ArgumentException("At least one parameter must be true");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Invalid arguments, try:");
            if (showCost) sb.AppendLine("cost <POWER_USAGE> <GAS_USAGE>");
            if (showUsage) sb.AppendLine("usage <TARIFF_NAME> <FUEL_TYPE> <TARGET_MONTHLY_SPEND>");
            return sb;
        }

        internal static (string tariffName, float cost)[] EvaluateCost(IDatasource ds, int powerUsage, int gasUsage)
        {
            List<(string, float cost)> @return = new List<(string, float)>(); // only need to explicitly name 'cost' at this point
            foreach (var tariff in ds.Tariffs)
            {
                if (Helpers.IsTariffApplicable(tariff.rates, powerUsage, gasUsage)) // won't use tariffs with rate == 0 if usage > 0
                {   
                    // keep costs separate for now so we can apply standing charge to each as needed
                    float powerCost = Helpers.CalculateCostFromUsage(tariff.rates.power, powerUsage);
                    float gasCost = Helpers.CalculateCostFromUsage(tariff.rates.gas, gasUsage);

                    float annualStandingCharge = Helpers.ConvertMonthlyToAnnual(tariff.standingCharge);

                    // need to do this separately as otherwise method couldn't distinguish used/unused utilities
                    float powerCostWithStandingCharge = Helpers.AddStandingCharge(annualStandingCharge, powerCost);
                    float gasCostWithStandingCharge = Helpers.AddStandingCharge(annualStandingCharge, gasCost);

                    // now can combine and finalise
                    float totalCostIncludingVAT = Helpers.AddVAT(powerCostWithStandingCharge + gasCostWithStandingCharge);

                    @return.Add((tariff.tariff, totalCostIncludingVAT));
                }
            }

            return @return.OrderBy(tariff => tariff.cost).ToArray();
        }

        internal static float EvaluateUsage(
            IDatasource ds, string tariffName, Constants.FuelType fuelType, int targetMonthlySpend, bool targetIncludesStandingCharge = true
            )
        {
            // assume all tariffs have unique names
            var tariff = ds.Tariffs.First(Helpers.TariffSelector(tariffName));

            float costExcludingVAT = Helpers.DeductVAT(targetMonthlySpend);

            // assuming the targetMonthlySpend includes a standing charge - but this is easily configurable in the app.config as it is not in the spec
            float costToCalculate = targetIncludesStandingCharge ? Helpers.DeductStandingCharge(tariff.standingCharge, costExcludingVAT) : costExcludingVAT;

            float rate = Helpers.SelectCorrectRate(fuelType, tariff.rates);

            float monthlyConsumption = Helpers.CalculateUsageFromCost(rate, costToCalculate);
            float annualConsumption = Helpers.ConvertMonthlyToAnnual(monthlyConsumption); // best to just do this once

            return annualConsumption;
        }
    }
}
