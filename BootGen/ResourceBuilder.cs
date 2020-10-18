using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    internal class ResourceBuilder
    {
        private readonly TypeBuilder typeBuilder;

        public ResourceBuilder(TypeBuilder typeBuilder)
        {
            this.typeBuilder = typeBuilder;
        }
        public Resource FromClass<T>()
        {
            return FromType(typeof(T));
        }


        private Resource FromType(Type type)
        {
            var result = new Resource();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];
            }

            result.Class = typeBuilder.FromType(type);
            result.Authenticate = type.CustomAttributes.Any(d => d.AttributeType == typeof(AuthenticateAttribute));
            result.IsReadonly = type.CustomAttributes.Any(d => d.AttributeType == typeof(ReadonlyAttribute));
            var generateAttr = type.CustomAttributes.FirstOrDefault(d => d.AttributeType == typeof(GenerateAttribute));
            if (generateAttr != null) {
                var args = generateAttr.ConstructorArguments.Select(a => (bool)a.Value).ToList();
                result.GenerationSettings.GenerateController = args[0];
                result.GenerationSettings.GenerateServiceInterface = args[1];
                result.GenerationSettings.GenerateService = args[2];
            }
            var controllerNameAttr = type.CustomAttributes.FirstOrDefault(d => d.AttributeType == typeof(ControllerNameAttribute));
            if (controllerNameAttr != null)
            {
                result.GenerationSettings.ControllerName = controllerNameAttr.ConstructorArguments.First().Value as string;
            }
            var serviceNameAttr = type.CustomAttributes.FirstOrDefault(d => d.AttributeType == typeof(ServiceNameAttribute));
            if (serviceNameAttr != null)
            {
                result.GenerationSettings.ServiceName = serviceNameAttr.ConstructorArguments.First().Value as string;
            }
            result.Class.IsResource = true;
            result.NestedResources = new List<Resource>();
            result.Name = result.Class.Name;
            return result;
        }
    }
}
