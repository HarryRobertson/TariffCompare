using System;
using System.Text;
using TariffCompare.Standard;

namespace TariffCompare
{
    class Program
    {
        private const string DATASOURCE = "Datasource";
        private const string TARGETMONTHLYCHARGE_INCLUDESSTANDINGCHARGE = "targetMonthlySpend_includesStandingCharge";

        private const string COST = "cost";
        private const string USAGE = "usage";

        static void Main(params string[] args)
        {
            string path = Config.Get(DATASOURCE);
            Datasource ds = new Datasource(path);

            StringBuilder sb = new StringBuilder();
            if (args.Length == 0)
                sb = GetHelp();
            else switch (args[0].ToLower())
            {
                case COST:
                    if (args.Length != 3 // guard against incorrect arguments
                            || !int.TryParse(args[1], out int powerUsage)
                            || !int.TryParse(args[2], out int gasUsage))
                        sb = GetHelp(true, false);
                    else
                    {
                        var costs = Functions.EvaluateCost(ds, powerUsage, gasUsage); // <-- PRIMARY FUNCTION
                        foreach (var cost in costs)
                        {
                            sb.AppendLine($"{cost.tariffName} {cost.cost:0.00}");
                        }
                    }
                    break;

                case USAGE:
                    if (args.Length != 4 // guard against incorrect arguments
                            || !Enum.TryParse(args[2], true, out CONSTANTS.FUELTYPE fuelType)
                            || !int.TryParse(args[3], out int targetMonthlySpend))
                        sb = GetHelp(false, true);
                    else
                    {
                        float usage;
                        if (bool.TryParse(Config.Get(TARGETMONTHLYCHARGE_INCLUDESSTANDINGCHARGE), out bool includesStandingCharge))
                            usage = Functions.EvaluateUsage(ds, args[1], fuelType, targetMonthlySpend, includesStandingCharge); // <-- PRIMARY FUNCTION
                        else // as it stands targetMonthlySpend_includesStandingCharge is not specified in the config, so the default mode will be used
                            usage = Functions.EvaluateUsage(ds, args[1], fuelType, targetMonthlySpend);
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

    }
}
