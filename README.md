# NUnitAllureBase

Using:
```CSharp
public abstract class ABase : NUnitAllureBase
{
    [OneTimeSetUp] public void OTS() => AllureBeforeAllTestsInClass();
    [OneTimeTearDown] public void OTT() => AllureAfterAllTestsInClass();
    [SetUp] public void S() => AllureBeforeTest();
    [TearDown] public void T() => AllureAfterTest();
}

public class TestClass1 : ABase
{
    [Test]
    public void Test2()
    {
        Step("This is a empty step");

        Step("This is a non empty step", () =>
        {
            TestContext.WriteLine("Some test log");
            Assert.That(1, Is.EqualTo(1));
        });
    }
}
```
