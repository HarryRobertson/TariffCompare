using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TariffCompare.Test")]
namespace TariffCompare.Standard
{
    internal static class Helpers
    {
        public static float AddVAT(float costExcludingVAT)
        {
            return costExcludingVAT * Constants.VAT;
        }

        public static float DeductVAT(float costIncludingVAT)
        {
            return costIncludingVAT / Constants.VAT;
        }

        public static float AddStandingCharge(float standingCharge, float costExludingStandingCharge)
        {
            float @return = costExludingStandingCharge;
            if (costExludingStandingCharge - 0 > Constants.FLOAT_DELTA)
            {   // only add the charge if the utility is actually being used
                @return += standingCharge;
            }
            return @return;
        }

        public static float DeductStandingCharge(float standingCharge, float costIncludingStandingCharge)
        {
            float @return = costIncludingStandingCharge - standingCharge;
            if (@return < 0f) // would only happen if the tool was being used wrong, but simple to check
                @return = 0f;
            return @return;
        }

        public static float CalculateCostFromUsage(float rate, float usage)
        {
            return usage * rate;
        }

        public static float CalculateUsageFromCost(float rate, float cost)
        {
            return cost / rate;
        }

        public static bool IsTariffApplicable((float power, float gas) rates, float powerUsage, float gasUsage)
        {
            bool @return = true;
            if (powerUsage > 0 && rates.power - 0 <= Constants.FLOAT_DELTA) @return = false; // if power is used but tariff does not have a rate
            if (gasUsage > 0 && rates.gas - 0 <= Constants.FLOAT_DELTA) @return = false;    // if gas is used but tariff does not have a rate
            return @return;
        }

        public static float ConvertMonthlyToAnnual(float monthlyCost)
        {
            return monthlyCost * 12;
        }

        public static float SelectCorrectRate(Constants.FuelType fuelType, (float power, float gas) rates)
        {
            float @return;
            switch (fuelType)
            {
                case Constants.FuelType.Power:
                    @return = rates.power;
                    break;
                case Constants.FuelType.Gas:
                    @return = rates.gas;
                    break;
                default:
                    throw new ArgumentException("fuelType should be one of either 'power' or 'gas'.");
            }
            return @return;
        }

        public static Func<(string tariff, (float power, float gas) rates, float standingCharge), bool> TariffSelector(string tariffName)
        {
            return tariff => tariff.tariff == tariffName;
        }
    }
}
