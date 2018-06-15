using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace TariffCompare.Standard.Test
{
    [TestFixture]
    public class FunctionTests
    {
        private class DatasourceStub : AbstractDatasource
        {
            private const string TEST_DATA = "[" + // same test data as main app, but non-configurable to keep tests valid
                                             "{\"tariff\": \"better-energy\", \"rates\": {\"power\":  0.1367, \"gas\": 0.0288}, \"standing_charge\": 8.33}, " +
                                             "{\"tariff\": \"2yr-fixed\", \"rates\": {\"power\": 0.1397, \"gas\": 0.0296}, \"standing_charge\": 8.75}, " +
                                             "{\"tariff\": \"greener-energy\", \"rates\": {\"power\":  0.1544}, \"standing_charge\": 8.33}, " +
                                             "{\"tariff\": \"simpler-energy\", \"rates\": {\"power\":  0.1396, \"gas\": 0.0328}, \"standing_charge\": 8.75}" +
                                             "]";

            public override (string tariff, (float power, float gas) rates, float standingCharge)[] Tariffs => ParseJson(TEST_DATA);
        }

        [TestCase(2000, 2300, "better-energy", 566.54f, "2yr-fixed", 585.35f, "simpler-energy", 592.87f)]
        [TestCase(2000, 0, "better-energy", 392.03f, "simpler-energy", 403.41f, "2yr-fixed", 403.62f, "greener-energy", 429.20f)]
        public void Test_EvaluateCosts_ReturnsCorrectNumberOfTariffsInCorrectOrder(int power, int gas, params object[] expectedTariffs)
        {
            var parsedExpectedTariffs = new List<(string tariffName, float cost)>(); // can't pass in Tuples via TestCase so have to do it this way
            for (int i = 0; i < expectedTariffs.Length; i+=2)
            {
                parsedExpectedTariffs.Add((expectedTariffs[i].ToString(), (float)expectedTariffs[i+1]));
            }
            var resultTariffs = Functions.EvaluateCost(new DatasourceStub(), power, gas);
            Assert.AreEqual(parsedExpectedTariffs.Count, resultTariffs.Length);

            for (int index = 0; index < parsedExpectedTariffs.Count; index++)
            {
                Assert.AreEqual(parsedExpectedTariffs[index].tariffName, resultTariffs[index].tariffName);
                Assert.AreEqual(parsedExpectedTariffs[index].cost, resultTariffs[index].cost, 0.005); // need to use large delta here due to rounded expected values
            }
        }

        [TestCase("greener-energy", "power", 40, 2313.36f)]
        [TestCase("better-energy", "gas", 25, 6449.80f)]
        public void Test_EvaluateUsage_ReturnsCorrectUsage(string tariffName, string fuelType, int targetMonthlySpend, float expectedUsage)
        {
            Enum.TryParse(fuelType, true, out CONSTANTS.FUELTYPE type); // I'm only testing targetIncludesStandingCharge == true here because...
            float usage = Functions.EvaluateUsage(new DatasourceStub(), tariffName, type, targetMonthlySpend); // ... I think that's the valid usecase

            Assert.AreEqual(expectedUsage, usage, 0.005); // need to use large delta here due to rounded expected values
        }
    }
}
