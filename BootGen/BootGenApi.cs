﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BootGen
{
    public class BootGenApi
    {
        private readonly SchemaStore schemaStore;
        private readonly ResourceBuilder resourceBuilder;
        public List<Resource> Resources { get; } = new List<Resource>();
        public List<Controller> Controllers { get; } = new List<Controller>();
        public List<Schema> Schemas => schemaStore.Schemas;

        public BootGenApi()
        {
            schemaStore = new SchemaStore();
            resourceBuilder = new ResourceBuilder(schemaStore);
        }
        public Resource AddResource<T>()
        {
            Resource resource = resourceBuilder.FromClass<T>();
            Resources.Add(resource);
            return resource;
        }
        public Controller AddController<T>()
        {
            var type = typeof(T);
            var controller = new Controller
            {
                Name = type.Name.Split('.').Last(),
                Methods = new List<Method>()
            };
            foreach (var method in type.GetMethods())
            {
                var controllerMethod = new Method
                {
                    Name = method.Name,
                    Parameters = new List<Property>()
                };
                controller.Methods.Add(controllerMethod);
                foreach (var param in method.GetParameters())
                {
                    var property = new SchemaBuilder(schemaStore).GetTypeDescription<Property>(param.ParameterType);
                    property.Name = param.Name;
                    controllerMethod.Parameters.Add(property);
                }

                TypeDescription responseType = new SchemaBuilder(schemaStore).GetTypeDescription<Property>(method.ReturnType);
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

        private static TypeDescription WrapType(string name, TypeDescription type)
        {
            return new TypeDescription
            {
                BuiltInType = BuiltInType.Object,
                IsCollection = false,
                Schema = new Schema
                {
                    Name = name,
                    Properties = new List<Property>
                    {
                        new Property
                        {
                            Name = "Value",
                            BuiltInType = type.BuiltInType,
                            IsCollection = type.IsCollection
                        }
                    }
                }
            };
        }
    }

    public class Controller
    {
        public string Name { get; set; }
        public List<Method> Methods { get; set; }
    }

    public class Method
    {
        public string Name { get; set; }
        public List<Property> Parameters { get; set; }
        public TypeDescription ReturnType { get; set; }
    }
}