using Allure.Commons;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitAllure
{
    public partial class NUnitAllureBase
    {
        private string testId => TestContext.CurrentContext.Test.ID;

        public void UpdateSuitName(string name)
        {
            Allure.UpdateTestCase(testId, t => t.labels.First(l => l.name == "suite").value = name);
        }

        public void UpdateTestName(string name)
        {
            Allure.UpdateTestCase(testId, t => t.name = name);
        }
    }
}

