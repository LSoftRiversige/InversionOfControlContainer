using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace InversionOfControlContainer
{
    public class ClassInjector
    {
        private Dictionary<string, object> ConstructorParamValues;
        private Dictionary<string, object> PropertyValues;

        public ClassInjector()
        {
            ConstructorParamValues = new Dictionary<string, object>();
            PropertyValues = new Dictionary<string, object>();
        }

        public void CheckDuplicateConstructorParamRegistration(string paramName)
        {
            if (ConstructorParamValues.ContainsKey(paramName))
            {
                throw new InvalidOperationException($"Parameter '{paramName}' already registered");
            }
        }

        public void AddParam(string paramName, object paramValue)
        {
            ConstructorParamValues[paramName] = paramValue;
        }

        public void CheckDuplicatePropertyRegistration(string propertyName)
        {
            if (PropertyValues.ContainsKey(propertyName))
            {
                throw new InvalidOperationException($"Property '{propertyName}' already registered");
            }
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

        public object GetValueByName(string name)
        {
            bool found = ConstructorParamValues.ContainsKey(name);
            if (!found)
            {
                throw new KeyNotFoundException($"Parameter {name} not found in value list");
            }
            return ConstructorParamValues[name];
        }

        public void AddProperty(string propertyName, object propertyValue)
        {
            PropertyValues[propertyName] = propertyValue;
        }
    }
}
