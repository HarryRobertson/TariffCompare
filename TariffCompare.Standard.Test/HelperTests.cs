using System;
using NUnit.Framework;
using TariffCompare.Standard;

namespace TariffCompare.Test
{
    [TestFixture]
    public class HelperTests
    {
        [TestCase(1f, 1.05f)]
        public void Test_AddVAT_AddsFivePercent(float input, float expected)
        {
            float output = Helpers.AddVAT(input);
            Assert.AreEqual(expected, output, Constants.FLOAT_DELTA); 
        }

        [TestCase(1.05f, 1f)]
        public void Test_DeductVAT_DeductsFivePercent(float input, float expected)
        {
            float output = Helpers.DeductVAT(input);
            Assert.AreEqual(expected, output, Constants.FLOAT_DELTA);
        }

        [TestCase(100f, 8f, 108f)]
        [TestCase(0f, 8f, 0f)]
        public void Test_AddStandingCharge_AddsCorrectAmountToCost(float cost, float standingCharge, float expected)
        {
            float output = Helpers.AddStandingCharge(standingCharge, cost);
            Assert.AreEqual(expected, output, Constants.FLOAT_DELTA);
        }

        [TestCase(8f, 108f, 100f)]
        [TestCase(8f, 4f, 0f)]
        public void Test_DeductStandingCharge_DeductsCorrectAmountFromCost(float standingCharge, float cost, float expected)
        {
            float output = Helpers.DeductStandingCharge(standingCharge, cost);
            Assert.AreEqual(expected, output, Constants.FLOAT_DELTA);
        }

        [TestCase(0.25f, 100f, 25f)]
        public void Test_CalculateCostFromUsage_MultipliesUsageByRate(float rate, float usage, float expected)
        {
            float output = Helpers.CalculateCostFromUsage(rate, usage);
            Assert.AreEqual(expected, output, Constants.FLOAT_DELTA);
        }

        [TestCase(0.25f, 100f, 400f)]
        public void Test_CalculateUsageFromCost_DividesCostByRate(float rate, float cost, float expected)
        {
            float output = Helpers.CalculateUsageFromCost(rate, cost);
            Assert.AreEqual(expected, output, Constants.FLOAT_DELTA);
        }

        [TestCase(0f, 0f, 100f, 100f, false)]
        [TestCase(0.1f, 0f, 100f, 100f, false)]
        [TestCase(0f, 0.1f, 100f, 100f, false)]
        [TestCase(0.1f, 0f, 100f, 0f, true)]
        [TestCase(0f, 0.1f, 0f, 100f, true)]
        [TestCase(0.1f, 0.1f, 100f, 100f, true)]
        [TestCase(0.1f, 0.1f, 0f, 100f, true)]
        [TestCase(0.1f, 0.1f, 100f, 0f, true)]
        public void Test_IsTariffApplicable_ReturnsFalseIfEitherRateIsZeroWhenUsageIsNonZero_OtherwiseTrue(
            float powerRate, float gasRate, float powerUsage, float gasUsage, bool expected
            )
        {   // simple method with lots of edge cases!
            bool output = Helpers.IsTariffApplicable((powerRate, gasRate), powerUsage, gasUsage);
            if (expected)
                Assert.IsTrue(output);
            else
                Assert.IsFalse(output);
        }

        [TestCase(12f, 144f)]
        public void Test_ConvertMonthlyToAnnual_MultipliesInputBy12(float input, float expected)
        {
            float output = Helpers.ConvertMonthlyToAnnual(input);
            Assert.AreEqual(expected, output, Constants.FLOAT_DELTA);
        }

        [TestCase("power", 0.01f, 0.02f, 0.01f)]
        [TestCase("gas", 0.01f, 0.02f, 0.02f)]
        public void Test_SelectCorrectRate_ReturnsCorrectRateForSelectedFuel(string fuelType, float powerRate, float gasRate, float expected)
        {
            Constants.FuelType type = (Constants.FuelType) Enum.Parse(typeof(Constants.FuelType), fuelType, true); // will throw and fail if not a valid type
            float output = Helpers.SelectCorrectRate(type, (powerRate, gasRate));
            Assert.AreEqual(expected, output, Constants.FLOAT_DELTA);
        }

        [TestCase("tariff1", "tariff1", true)]
        [TestCase("tariff1", "tariff2", false)]
        public void Test_TariffSelector_ReturnedFunction_ReturnsTrueWhenPassedMatchingTariff_OtherwiseFalse(
            string testTariff, string comparisonTariff, bool expected
            )
        {
            var output = Helpers.TariffSelector(comparisonTariff); // gets a function that tests tariffs for the matching name...
            var tariffStub = (testTariff, (0f, 0f), 0f);
            if (expected) 
                Assert.IsTrue(output(tariffStub));
            else // ... so we test the result of the function
                Assert.IsFalse(output(tariffStub));
        }
    }
}
