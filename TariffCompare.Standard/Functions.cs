using System.Collections.Generic;
using System.Linq;

namespace TariffCompare.Standard
{
    public class Functions
    {
        public static (string tariffName, float cost)[] EvaluateCost(IDatasource ds, int powerUsage, int gasUsage)
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

        public static float EvaluateUsage(
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
