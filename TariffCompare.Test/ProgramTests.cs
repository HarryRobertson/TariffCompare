using System;
using NUnit.Framework;
using TariffCompare.Standard;

namespace TariffCompare.Test
{
    [TestFixture]
    public class ProgramTests
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

        [TestCase(2000, 2300, 566.54f, 585.35f, 592.87f)]
        [TestCase(2000, 0, 392.03f, 403.41f, 403.62f, 429.20f)]
        public void Test_EvaluateCosts_ReturnsCorrectNumberOfTariffsInCorrectOrder(int power, int gas, params float[] expectedCosts)
        {
            var tariffCosts = Functions.EvaluateCost(new DatasourceStub(), power, gas);
            Assert.AreEqual(expectedCosts.Length, tariffCosts.Length);

            for (int index = 0; index < expectedCosts.Length; index++)
            {
                Assert.AreEqual(expectedCosts[index], tariffCosts[index].cost, 0.005); // need to use large delta here due to rounded expected values
            }
        }

        [TestCase("greener-energy", "power", 40, 2313.36f)]
        [TestCase("better-energy", "gas", 25, 6449.80f)]
        public void Test_EvaluateUsage_ReturnsCorrectUsage(string tariffName, string fuelType, int targetMonthlySpend, float expectedUsage)
        {
            Enum.TryParse(fuelType, true, out Constants.FuelType type);
            float usage = Functions.EvaluateUsage(new DatasourceStub(), tariffName, type, targetMonthlySpend);

            Assert.AreEqual(expectedUsage, usage, 0.005); // need to use large delta here due to rounded expected values
        }
    }
}
