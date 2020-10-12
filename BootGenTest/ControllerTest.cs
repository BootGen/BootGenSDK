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
            bool PrimitiveTestFunction();

            [Post]
            Dummy TestFunctionWithBody(Dummy dummy);
        }

        [TestMethod]
        public void SimpleControllerTest()
        {
            var dataModel = new DataModel();
            var collection = new ControllerCollection(dataModel);
            var controller = collection.Add<TestController>();
            var api = new Api(new ResourceCollection(dataModel), collection);
            var primitiveTestFunction = controller.Methods.First();

            Assert.AreEqual("PrimitiveTestFunction", primitiveTestFunction.Name);
            Assert.AreEqual(HttpVerb.Get, primitiveTestFunction.Verb);
            Assert.AreEqual(BuiltInType.Bool, primitiveTestFunction.ReturnType.Class.Properties.First().BuiltInType);

            var testFunctionWithBody = controller.Methods.Last();
            Assert.AreEqual("TestFunctionWithBody", testFunctionWithBody.Name);
            Assert.AreEqual("dummy", testFunctionWithBody.Parameter.Name);
            Assert.AreEqual("Dummy", testFunctionWithBody.Parameter.Class.Name.Singular);
            Assert.AreEqual("Dummy", testFunctionWithBody.ReturnType.Class.Name.Singular);
        }
    }
}