using Allure.Commons;
using NUnit.Framework.Internal;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace NUnitAllure
{
    public partial class NUnitAllureBase
    {
        //private ConcurrentDictionary<string, int> stepLogLenghtContainer = new ConcurrentDictionary<string, int>();

        public void Step(string name) => Step(name, () => { });

        public void Step(string name, Action action)
        {
            var caseId = TestExecutionContext.CurrentContext.CurrentTest.Id;
            var uuid = Guid.NewGuid().ToString("N");
            Allure.StartStep(caseId, uuid, new StepResult()
            {
                name = name
            });

            //steps.AddOrUpdate(caseId, uuid, (k, v) => uuid);
            int logLength = TestExecutionContext.CurrentContext.CurrentResult.Output.Length;
            Exception stepException = null;
            try
            {
                action();
            }
            catch (Exception e)
            {
                stepException = e;
            }

            string stepLog = TestExecutionContext.CurrentContext.CurrentResult.Output.Remove(0, logLength);
            AddStepLog(uuid, stepLog);

            Allure.UpdateStep(uuid, x =>
            {
                x.status = GetStatusFromException(stepException);
                x.statusDetails = new StatusDetails
                {
                    message = stepException?.Message,
                    trace = stepException?.StackTrace
                };
                x.attachments.AddRange(AllureHelper.GetAttaches());
            });
            Allure.StopStep(uuid);
            //steps.TryRemove(caseId, out uuid);

            if (stepException != null)
                throw stepException;
        }

        private void AddStepLog(string uuid, string log)
        {
            if (!String.IsNullOrEmpty(log))
            {
                string attachFile = Guid.NewGuid().ToString("N") + "-step.log";
                try
                {
                    System.IO.File.WriteAllText(System.IO.Path.Combine(Allure.ResultsDirectory, attachFile), log);
                }
                catch (Exception)
                {
                    return;
                }

                Allure.UpdateStep(uuid, x =>
                {
                    x.attachments.Add(new Attachment()
                    {
                        name = "StepLog",
                        source = attachFile,
                        type = "text/plain"
                    });
                });
            }
        }

        private Status GetStatusFromException(Exception e)
        {
            if (e == null || e is SuccessException)
                return Status.passed;
            if (e is IgnoreException)
                return Status.skipped;
            if (e is InconclusiveException)
                return Status.none;
            if (e.GetType().ToString().Contains("Assert"))
                return Status.failed;

            return Status.broken;
        }
    }
}
