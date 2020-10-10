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

        public Controller Add<T>(bool authenticate = false)
        {
            var type = typeof(T);
            var controller = new Controller
            {
                Name = type.Name.Split('.').Last(),
                Methods = new List<Method>(),
                Authenticate = authenticate
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
                    Parameters = new List<Property>(),
                    Verb = verb
                };
                controller.Methods.Add(controllerMethod);
                foreach (var param in method.GetParameters())
                {
                    var property = DataModel.TypeBuilder.GetProperty(param.ParameterType);
                    property.Name = param.Name;
                    controllerMethod.Parameters.Add(property);
                }

                TypeDescription responseType = DataModel.TypeBuilder.GetProperty(method.ReturnType);
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