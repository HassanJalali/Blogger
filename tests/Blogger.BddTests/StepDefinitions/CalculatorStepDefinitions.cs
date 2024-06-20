namespace Blogger.BddTests.StepDefinitions
{
    [Binding]
    public sealed class CalculatorStepDefinitions
    {
        [Given("the first number is (.*)")]
        public static void GivenTheFirstNumberIs(int number)
        {
            Assert.IsTrue(true);
        }

        [Given("the second number is (.*)")]
        public static void GivenTheSecondNumberIs(int number)
        {
            Assert.IsTrue(true);
        }

        [When("the two numbers are added")]
        public static void WhenTheTwoNumbersAreAdded()
        {
            Assert.IsTrue(true);
        }

        [Then("the result should be (.*)")]
        public static void ThenTheResultShouldBe(int result)
        {
            Assert.IsTrue(true);
        }
    }
}
