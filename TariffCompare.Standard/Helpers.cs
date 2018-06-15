using System;

namespace TariffCompare.Standard
{
    internal static class Helpers
    {
        public static float AddVAT(float costExcludingVAT)
        {
            return costExcludingVAT * CONSTANTS.VAT;
        }

        public static float DeductVAT(float costIncludingVAT)
        {
            return costIncludingVAT / CONSTANTS.VAT;
        }

        public static float AddStandingCharge(float standingCharge, float costExludingStandingCharge)
        {
            float @return = costExludingStandingCharge;
            if (costExludingStandingCharge - 0 > CONSTANTS.FLOAT_DELTA)
            {   // only add the charge if the utility is actually being used
                @return += standingCharge;
            }
            return @return;
        }

        public static float DeductStandingCharge(float standingCharge, float costIncludingStandingCharge)
        {
            float @return = costIncludingStandingCharge - standingCharge;
            if (@return < 0f) // would only happen if the tool was being used wrong, but simple to check so might as well
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
            if (powerUsage > 0 && rates.power - 0 <= CONSTANTS.FLOAT_DELTA) @return = false; // if power is used but tariff does not have a rate
            if (gasUsage > 0 && rates.gas - 0 <= CONSTANTS.FLOAT_DELTA) @return = false;    // if gas is used but tariff does not have a rate
            return @return;
        }

        public static float ConvertMonthlyToAnnual(float monthlyCost)
        {
            return monthlyCost * 12;
        }

        public static float SelectCorrectRate(CONSTANTS.FUELTYPE fueltype, (float power, float gas) rates)
        {
            float @return;
            switch (fueltype)
            {
                case CONSTANTS.FUELTYPE.POWER:
                    @return = rates.power;
                    break;
                case CONSTANTS.FUELTYPE.GAS:
                    @return = rates.gas;
                    break;
                default:
                    throw new ArgumentException("fueltype should be one of either 'CONSTANTS.FUELTYPE.POWER' or 'CONSTANTS.FUELTYPE.GAS'.");
            }
            return @return;
        }

        public static Func<(string tariff, (float power, float gas) rates, float standingCharge), bool> TariffSelector(string tariffName)
        {
            return tariff => tariff.tariff == tariffName;
        }
    }
}
