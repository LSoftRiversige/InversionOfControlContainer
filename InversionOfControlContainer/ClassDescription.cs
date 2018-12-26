using System;
using System.Collections.Generic;
using System.Text;

namespace InversionOfControlContainer
{
    public class ClassDescription : IClassDescription
    {
        public Type ObjectType { get; set; }
        public Dictionary<string, object> ConstructorParamValues { get; set; }

        public void WithConstructorArgument(string paramName, object paramValue)
        {
            if (ConstructorParamValues == null)
            {
                ConstructorParamValues = new Dictionary<string, object>();
            }
            if (ConstructorParamValues.ContainsKey(paramName))
            {
                throw new InvalidOperationException($"Parameter {paramName} already registered");
            }
            ConstructorParamValues[paramName] = paramValue;
        }
    }
}
