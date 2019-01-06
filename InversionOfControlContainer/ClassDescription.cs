using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace InversionOfControlContainer
{
    public class ClassDescription : IClassDescription
    {
        public Type ObjectType { get; set; }

        private ClassInjector injector;

        public ClassDescription()
        {
            injector = new ClassInjector();
        }

        public IClassDescription WithConstructorArgument(string paramName, object paramValue)
        {
            injector.CheckDuplicateConstructorParamRegistration(paramName);
            CheckConstructorWithParamExists(paramName);
            injector.AddParam(paramName, paramValue);
            return this;
        }

        private void CheckConstructorWithParamExists(string paramName)
        {
            ConstructorInfo constructorWithTheParam = ObjectType
                .GetConstructors()
                .FirstOrDefault(c => c.GetParameters()
                   .Any(p => p.Name.Equals(paramName)));

            if (constructorWithTheParam == null)
            {
                throw new InvalidOperationException($"No constructor was found with parameter '{paramName}' in the class '{ClassName()}'");
            }
        }

        private string ClassName()
        {
            return ObjectType.Name;
        }

        public IClassDescription WithPropertyValue(string propertyName, object propertyValue)
        {
            injector.CheckDuplicatePropertyRegistration(propertyName);
            CheckPropertyExists(propertyName);
            injector.AddProperty(propertyName, propertyValue);
            return this;
        }

        private void CheckPropertyExists(string propertyName)
        {
            if (ObjectType.GetProperty(propertyName) == null)
            {
                throw new InvalidOperationException($"Property '{propertyName}' not found in class '{ClassName()}'");
            }
        }

        public void InjectPropertiesToObject(object obj)
        {
            PropertyInfo[] properties = ObjectType.GetProperties();
            foreach (var prop in properties)
            {
                injector.TryInjectProperty(prop, obj);
            }
        }

        public object GetValueByName(string name)
        {
            return injector.GetValueByName(name);
        }
    }
}