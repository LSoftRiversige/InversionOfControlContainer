using System;
using System.Collections.Generic;
using System.Reflection;

namespace InversionOfControlContainer
{
    public interface IClassDescription
    {
        Type ObjectType { get; set; }

        IClassDescription WithConstructorArgument(string paramName, object paramValue);
        IClassDescription WithPropertyValue(string propertyName, object value);
        void InjectPropertiesToObject(object obj);
        object GetValueByName(string name);
    }
}