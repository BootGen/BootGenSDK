using System;
using System.Collections.Generic;
using System.Linq;
using BootGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BootGenTest
{
    [TestClass]
    public class ControllerTest
    {
        class Dummy
        {
            public string Name { get; set; }
        }
        interface TestController
        {
            [Get]
            bool PrimitiveTestFunction(string param1, int param2);

            [Post]
            Dummy TestFunctionWithBody(Dummy dummy);
        }

        [TestMethod]
        public void SimpleControllerTest()
        {
            var api = new BootGenApi(new ResourceStore());
            var controller = api.AddController<TestController>();
            var primitiveTestFunction = controller.Methods.First();

            Assert.AreEqual("PrimitiveTestFunction", primitiveTestFunction.Name);
            Assert.AreEqual("param1", primitiveTestFunction.Parameters.First().Name);
            Assert.AreEqual(BuiltInType.String, primitiveTestFunction.Parameters.First().BuiltInType);
            Assert.AreEqual("param2", primitiveTestFunction.Parameters.Last().Name);
            Assert.AreEqual(BuiltInType.Int32, primitiveTestFunction.Parameters.Last().BuiltInType);
            Assert.AreEqual(HttpVerb.Get, primitiveTestFunction.Verb);
            Assert.AreEqual(BuiltInType.Bool, primitiveTestFunction.ReturnType.Class.Properties.First().BuiltInType);

            var testFunctionWithBody = controller.Methods.Last();
            Assert.AreEqual("TestFunctionWithBody", testFunctionWithBody.Name);
            Assert.AreEqual("dummy", testFunctionWithBody.Parameters.First().Name);
            Assert.AreEqual("Dummy", testFunctionWithBody.Parameters.First().Class.Name);
            Assert.AreEqual("Dummy", testFunctionWithBody.ReturnType.Class.Name);
        }
    }
}