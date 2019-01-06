﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace InversionOfControlContainer
{
    public interface IClassDescription
    {
        Dictionary<string, object> ConstructorParamValues { get; set; }
        Type ObjectType { get; set; }

        IClassDescription WithConstructorArgument(string paramName, object paramValue);
        IClassDescription WithPropertyValue(string propertyName, object value);
        bool TryInjectProperty(PropertyInfo property, object obj);
    }
}