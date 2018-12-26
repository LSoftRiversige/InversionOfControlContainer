using System;
using System.Collections.Generic;

namespace InversionOfControlContainer
{
    public interface IClassDescription
    {
        Dictionary<string, object> ConstructorParamValues { get; set; }
        Type ObjectType { get; set; }

        void WithConstructorArgument(string paramName, object paramValue);
    }
}