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


        public Resource FromType(Type type)
        {
            var result = new Resource();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];
            }

            result.Class = typeBuilder.FromType(type);
            result.Authenticate = type.Has<AuthenticateAttribute>();
            result.IsReadonly = type.Has<ReadonlyAttribute>();
            var generateAttr = type.Get<GenerateAttribute>();
            if (generateAttr != null) {
                var args = generateAttr.ConstructorArguments.Select(a => (bool)a.Value).ToList();
                result.GenerationSettings.GenerateController = args[0];
                result.GenerationSettings.GenerateServiceInterface = args[1];
                result.GenerationSettings.GenerateService = args[2];
            }
            var controllerNameAttr = type.Get<ControllerNameAttribute>();
            if (controllerNameAttr != null)
            {
                result.GenerationSettings.ControllerName = controllerNameAttr.GetFirstParameter<string>();
            }
            var serviceNameAttr = type.Get<ServiceNameAttribute>();
            if (serviceNameAttr != null)
            {
                result.GenerationSettings.ServiceName = serviceNameAttr.GetFirstParameter<string>();
            }
            result.Class.IsResource = true;
            result.NestedResources = new List<Resource>();
            result.Name = result.Class.Name;
            return result;
        }
    }
}
