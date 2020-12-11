﻿using System.Net;
using System.Threading.Tasks;

using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Webservices;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Webservices
{

    #region Supporting data structures

    public class TestResource
    {

        [ResourceMethod("task")]
        public Task AsyncTask() => Task.CompletedTask;

        [ResourceMethod("value-task")]
        public ValueTask AsyncValueTask() => ValueTask.CompletedTask;

        [ResourceMethod("generic-task")]
        public Task<string> AsyncGenericTask() => Task.FromResult("Task result");

        [ResourceMethod("generic-value-task")]
        public ValueTask<string> AsyncGenericValueTask() => ValueTask.FromResult("ValueTask result");

    }

    #endregion

    [TestClass]
    public class ResultTypeTests
    {

        #region Tests

        [TestMethod]
        public void ControllerMayReturnTask()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/task");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public void ControllerMayReturnValueTask()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/value-task");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public void ControllerMayReturnGenericTask()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/generic-task");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Task result", response.GetContent());
        }

        [TestMethod]
        public void ControllerMayReturnGenericValueTask()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/generic-value-task");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("ValueTask result", response.GetContent());
        }

        #endregion

        #region Helpers

        private TestRunner GetRunner()
        {
            return TestRunner.Run(Layout.Create().AddService<TestResource>("t"));
        }

        #endregion

    }

}
