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
        public Dictionary<string, object> ConstructorParamValues { get; set; }
        public Dictionary<string, object> PropertyValues { get; set; }

        public IClassDescription WithConstructorArgument(string paramName, object paramValue)
        {
            TryCreateConstructorParamValues();
            CheckDuplicateConstructorParamRegistration(paramName);
            CheckConstructorWithParamExists(paramName);
            ConstructorParamValues[paramName] = paramValue;
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

        private void CheckDuplicateConstructorParamRegistration(string paramName)
        {
            if (ConstructorParamValues.ContainsKey(paramName))
            {
                throw new InvalidOperationException($"Parameter '{paramName}' already registered");
            }
        }

        private bool TryCreateConstructorParamValues()
        {
            if (ConstructorParamValues == null)
            {
                ConstructorParamValues = new Dictionary<string, object>();
                return true;
            }
            return false;
        }

        public IClassDescription WithPropertyValue(string propertyName, object propertyValue)
        {
            TryCreatePropertyValues();
            CheckDuplicatePropertyRegistration(propertyName);
            CheckPropertyExists(propertyName);
            PropertyValues[propertyName] = propertyValue;
            return this;
        }

        private void CheckPropertyExists(string propertyName)
        {
            if (ObjectType.GetProperty(propertyName) == null)
            {
                throw new InvalidOperationException($"Property '{propertyName}' not found in class '{ClassName()}'");
            }
        }

        private void CheckDuplicatePropertyRegistration(string propertyName)
        {
            if (PropertyValues.ContainsKey(propertyName))
            {
                throw new InvalidOperationException($"Property '{propertyName}' already registered");
            }
        }

        private bool TryCreatePropertyValues()
        {
            if (PropertyValues == null)
            {
                PropertyValues = new Dictionary<string, object>();
                return true;
            }
            return false;
        }

        public bool TryInjectProperty(PropertyInfo property, object obj)
        {
            if (PropertyValues == null)
            {
                return false;
            }
            if (PropertyValues.ContainsKey(property.Name))
            {
                var value = PropertyValues[property.Name];
                property.SetValue(obj, value);
                return true;
            }
            return false;
        }
    }
}
