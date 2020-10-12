using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BootGen
{
    public class ControllerCollection
    {
        public DataModel DataModel { get; }
        public List<Controller> Controllers { get; } = new List<Controller>();

        public ControllerCollection(DataModel dataModel)
        {
            DataModel = dataModel;
        }

        public Controller Add<T>()
        {
            var type = typeof(T);
            var controller = new Controller
            {
                Name = type.Name.Split('.').Last(),
                Methods = new List<Method>()
            };
            foreach (var method in type.GetMethods())
            {
                var verb = HttpVerb.Post;
                if (method.CustomAttributes.Any(a => a.AttributeType == typeof(GetAttribute)))
                    verb = HttpVerb.Get;
                if (method.CustomAttributes.Any(a => a.AttributeType == typeof(PostAttribute)))
                    verb = HttpVerb.Post;
                if (method.CustomAttributes.Any(a => a.AttributeType == typeof(PatchAttribute)))
                    verb = HttpVerb.Patch;
                if (method.CustomAttributes.Any(a => a.AttributeType == typeof(PutAttribute)))
                    verb = HttpVerb.Put;
                if (method.CustomAttributes.Any(a => a.AttributeType == typeof(DeleteAttribute)))
                    verb = HttpVerb.Delete;
                var controllerMethod = new Method
                {
                    Name = method.Name,
                    Verb = verb
                };
                controller.Methods.Add(controllerMethod);
                var typeBuilder = DataModel.NonPersistedTypeBuilder;
                ParameterInfo[] parameters = method.GetParameters();
                if (verb == HttpVerb.Get && parameters.Length > 0)
                    throw new Exception("Get controller methods might not have parameters.");
                if (parameters.Length > 1)
                    throw new Exception("Controller methods might have maximal one parameter.");
                if (parameters.Length == 1)
                {
                    var param = parameters.First();
                    controllerMethod.Parameter = typeBuilder.GetProperty<Parameter>(param.ParameterType);
                    if (controllerMethod.Parameter.BuiltInType != BuiltInType.Object)
                        throw new Exception("Controller method parameter must be a custom object.");
                    controllerMethod.Parameter.Name = param.Name;
                }      

                var responseType = typeBuilder.GetProperty<TypeDescription>(method.ReturnType);
                if (responseType.BuiltInType == BuiltInType.Object)
                {
                    controllerMethod.ReturnType = responseType;
                }
                else
                {
                    controllerMethod.ReturnType = WrapType(method.Name + "Response", responseType);
                }
            }
            Controllers.Add(controller);
            return controller;
        }

        private TypeDescription WrapType(string name, TypeDescription type)
        {
            var c = new ClassModel
            {
                Name = name,
                Properties = new List<Property>
                    {
                        new Property
                        {
                            Name = "Value",
                            BuiltInType = type.BuiltInType,
                            IsCollection = type.IsCollection,
                            IsRequired = true
                        }
                    }
            };
            DataModel.ClassCollection.Add(c);
            return new TypeDescription
            {
                BuiltInType = BuiltInType.Object,
                IsCollection = false,
                Class = c
            };
        }
    }
}