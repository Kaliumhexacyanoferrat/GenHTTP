﻿using GenHTTP.Modules.IO;
using GenHTTP.Modules.Razor;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Razor
{

    [TestClass]
    public sealed class ConfigurationTests
    {

        [TestMethod]
        public void TestLinq()
        {
            var template = Resource.FromString("100 = @Enumerable.Range(0, 100).Count()");

            var page = ModRazor.Page(template)
                               .AddAssemblyReference("System.Linq")
                               .AddUsing("System.Linq");

            using var runner = TestRunner.Run(page);

            using var response = runner.GetResponse();

            AssertX.Contains("100 = 100", response.GetContent());
        }

    }

}
