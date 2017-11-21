using NUnit.Framework;
using System;
using NUnitAllure;

namespace NUnitAllureBaseTests
{
    public class Tests
    {
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
            public void Test1()
            {
                TestContext.WriteLine("Some text");
                TestContext.WriteLine("Some text 2");
            }

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

            [Test]
            public void Test3Failed()
            {
                Step("This is the 1st step", ()=>
                {
                    Assert.That(1, Is.EqualTo(1));
                });

                Step("This is the 2nd step", () =>
                {
                    Assert.That(1, Is.EqualTo(2), "Some message");
                });
            }

            [Test]
            public void Test4Broken()
            {
                Step("This is the 1st step", () =>
                {
                    Assert.That(1, Is.EqualTo(1));
                });

                Step("This is the 2nd step", () =>
                {
                    var x = 0;
                    var y = 1 / x;
                });
            }

            [Test]
            public void Test5Attach()
            {
                Step("This is the 1st step", () =>
                {
                    AllureHelper.AttachText("This goes to step", "Some text");
                });

                AllureHelper.AttachText("This goes to 2nd step", "Some text 2");

                Step("This is the 2nd step", () =>
                {
                    AllureHelper.AttachText("This goes to 2n step too", "Some text 3");
                });

                AllureHelper.AttachText("This goes to case", "Some text 4");
            }

            [Test]
            public void Test6DifferentAttaches()
            {
                AllureHelper.AttachText("This is a text", "Some text");
                AllureHelper.AttachJson("This is a json", "{\"a\": true}");
                AllureHelper.AttachXml("This is a xml", "<xxx>Some node</xxx>");
                AllureHelper.AttachPng("This is a png", System.IO.File.ReadAllBytes(@"c:\TestPictures\temp.png"));
            }

            [Test]
            public void Test7StepLog()
            {
                TestContext.WriteLine("This goes to no step");

                Step("This is the 1st step", () =>
                {
                    TestContext.WriteLine("This goes to 1st step 2");
                });

                TestContext.WriteLine("This goes to no step 1");
                TestContext.WriteLine("This goes to no step 2");

                Step("This is the 2nd step", () =>
                {
                    TestContext.WriteLine("This goes to 2nd step 1");
                    TestContext.WriteLine("This goes to 2nd step 2");
                });

                TestContext.WriteLine("This goes to no step 3");
            }
        }
    }
}
