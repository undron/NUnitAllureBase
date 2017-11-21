using Allure.Commons;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NUnitAllure
{
    public partial class NUnitAllureBase
    {
        private static AllureLifecycle Allure => AllureLifecycle.Instance;
        private List<string> caseIds = new List<string>();

        //[OneTimeSetUp]
        protected void AllureBeforeAllTestsInClass()
        {
            //TestContext.Progress.WriteLine($"Writing results to {Allure.ResultsDirectory}");
            var nunitFixture = TestExecutionContext.CurrentContext.CurrentTest;

            var fixture = new TestResultContainer
            {
                uuid = nunitFixture.Id,
                name = nunitFixture.ClassName
            };
            Allure.StartTestContainer(fixture);
        }

        //[OneTimeTearDown]
        protected void AllureAfterAllTestsInClass()
        {
            var nunitFixture = TestExecutionContext.CurrentContext.CurrentTest;
            var casesShouldBeRunned = TestExecutionContext.CurrentContext.CurrentResult.Children.Select(r => r.Test.Id).Where(id => !caseIds.Contains(id));
            if (casesShouldBeRunned.Any())
                AddMissedCases(casesShouldBeRunned);

            Allure.StopTestContainer(nunitFixture.Id);
            Allure.WriteTestContainer(nunitFixture.Id);
        }

        private void AddMissedCases(IEnumerable<string> missedCaseIds)
        {
            var results = TestExecutionContext.CurrentContext.CurrentResult.Children;

            foreach (var id in missedCaseIds)
            {
                var result = results.First(r => r.Test.Id == id);

                var testResult = new Allure.Commons.TestResult
                {
                    uuid = id,
                    historyId = result.Test.FullName,
                    name = result.Test.MethodName,
                    fullName = result.Test.FullName,
                    labels = new List<Label>
                    {
                        Label.Suite(result.Test.ClassName),
                        Label.Thread(),
                        Label.Host(),
                        Label.TestClass(result.Test.ClassName),
                        Label.TestMethod(result.Test.MethodName),
                        Label.Package(result.Test.Fixture?.ToString() ?? result.Test.Parent.Fixture.ToString())
                    },
                    status = GetNunitStatus(result.ResultState),
                    statusDetails = new StatusDetails
                    {
                        message = result.Message,
                        trace = result.StackTrace
                    }
                };

                Allure.StartTestCase(testResult);
                Allure.StopTestCase(id);
                Allure.WriteTestCase(id);
            }
        }

        //[SetUp]
        protected void AllureBeforeTest()
        {
            var nunitTest = TestExecutionContext.CurrentContext.CurrentTest;
            SaveCaseId(nunitTest.Id);

            var testResult = new Allure.Commons.TestResult
            {
                uuid = nunitTest.Id,
                historyId = nunitTest.FullName,
                name = nunitTest.MethodName,
                fullName = nunitTest.FullName,
                labels = new List<Label>
                    {
                        Label.Suite(nunitTest.ClassName),
                        Label.Thread(),
                        Label.Host(),
                        Label.TestClass(nunitTest.ClassName),
                        Label.TestMethod(nunitTest.MethodName),
                        Label.Package(nunitTest.Fixture.ToString())
                    }
            };

            Allure.StartTestCase(testResult);
        }

        private void SaveCaseId(string caseId)
        {
            lock (caseIds)
            {
                caseIds.Add(caseId);
            }
        }

        //[TearDown]
        protected void AllureAfterTest()
        {
            var nunitTest = TestExecutionContext.CurrentContext.CurrentTest;

            AttachTestLog(nunitTest.Id);
            Allure.UpdateTestCase(nunitTest.Id, x =>
            {
                x.statusDetails = new StatusDetails
                {
                    message = TestContext.CurrentContext.Result.Message,
                    trace = TestContext.CurrentContext.Result.StackTrace
                };
                x.status = GetNunitStatus(TestContext.CurrentContext.Result.Outcome);
                x.attachments.AddRange(AllureHelper.GetAttaches());
            });
            
            Allure.StopTestCase(nunitTest.Id);
            Allure.WriteTestCase(nunitTest.Id);
        }

        private void AttachTestLog(string caseId)
        {
            var logAttach = GetTestLog();
            if (logAttach != null)
                Allure.UpdateTestCase(caseId, x => x.attachments.Add(logAttach));
        }

        private Attachment GetTestLog()
        {
            string testOutput = TestExecutionContext.CurrentContext.CurrentResult.Output;
            string attachFile = Guid.NewGuid().ToString("N") + ".log";
            try
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(Allure.ResultsDirectory, attachFile), testOutput);
            }
            catch (Exception)
            {
                return null;
            }

            return new Attachment()
            {
                name = "Output",
                source = attachFile,
                type = "text/plain"
            };
        }

        private static Status GetNunitStatus(ResultState result)
        {
            switch (result.Status)
            {
                case TestStatus.Inconclusive:
                    return Status.broken;
                case TestStatus.Skipped:
                    return Status.skipped;
                case TestStatus.Passed:
                    return Status.passed;
                case TestStatus.Warning:
                    return Status.broken;
                case TestStatus.Failed:
                    if (String.IsNullOrEmpty(result.Label))
                        return Status.failed;
                    else
                        return Status.broken;
                default:
                    return Status.none;
            }
        }
    }
}
