using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

namespace InversionOfControlContainer
{
    public class Container : IContainer
    {
        private readonly Dictionary<Type, IClassDescription> bindings;
        private Type lastBinding;

        public Dictionary<Type, IClassDescription> Bindings => bindings;

        public Container()
        {
            bindings = new Dictionary<Type, IClassDescription>();
        }

        public IContainer Bind<TKey, TValue>()
        {
            if (Bindings.ContainsKey(typeof(TKey)))
            {
                throw new InvalidOperationException($"Duplicate register of binding {nameof(TKey)} => {nameof(TValue)}");
            }
            lastBinding = typeof(TKey);
            Bindings[lastBinding] = new ClassDescription() { ObjectType = typeof(TValue) };
            return this;
        }

        public IContainer WithConstructorArgument(string paramName, object paramValue)
        {
            if (lastBinding == null)
            {
                throw new InvalidOperationException("Method 'WithConstructorArgument' must called after 'Bind'");
            }
            IClassDescription classDescription = Bindings[lastBinding];
            classDescription.WithConstructorArgument(paramName, paramValue);
            return this;
        }

        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }

        private object Get(Type t, HashSet<Type> stack = null)
        {
            if (t.IsClass)
            {
                return Activator.CreateInstance(t);
            }
            if (t.IsInterface)
            {
                if (stack == null)
                {
                    stack = new HashSet<Type> { t };
                }
                IClassDescription theClass = DefineClassByInterface(t);
                object[] ctorParams = DefineConstructorParams(theClass, stack);
                return Activator.CreateInstance(theClass.ObjectType, ctorParams);
            }
            return null;
        }

        private object[] DefineConstructorParams(IClassDescription theClass, HashSet<Type> stack)
        {
            //first constructor with minimum parameters
            ConstructorInfo constructor = theClass.ObjectType.GetConstructors().OrderBy(c => c.GetParameters().Count()).First();

            ParameterInfo[] parameters = constructor.GetParameters();
            return GetParamValues(parameters, theClass, stack);
        }

        private object[] GetParamValues(ParameterInfo[] parameters, IClassDescription theClass, HashSet<Type> stack)
        {
            object[] result = new object[parameters.Length];

            int i = 0;
            foreach (ParameterInfo p in parameters)
            {
                result[i++] = DefineParamValue(theClass, p, stack);
            }
            return result;
        }

        private object DefineParamValue(IClassDescription theClass, ParameterInfo p, HashSet<Type> stack)
        {
            Type typeOfParam = p.ParameterType;
            TypeCode typeCode = Type.GetTypeCode(typeOfParam);
            if (typeCode == TypeCode.Object)
            {
                CheckStackOverflow(stack, typeOfParam);
                return Get(typeOfParam, stack);
            }
            else
            {
                return GetValueByName(p.Name, theClass);
            }
        }

        private static void CheckStackOverflow(HashSet<Type> stack, Type typeOfParam)
        {
            if (stack.Contains(typeOfParam))
            {
                throw new InvalidOperationException($"Circular class reference for class '{typeOfParam.Name}'");
            }
        }

        private object GetValueByName(string name, IClassDescription classDescription)
        {
            if (!classDescription.ConstructorParamValues.ContainsKey(name))
            {
                throw new KeyNotFoundException($"Parameter {name} not found in value list");
            }
            return classDescription.ConstructorParamValues[name];
        }

        private IClassDescription DefineClassByInterface(Type typeOfInterface)
        {
            if (!bindings.ContainsKey(typeOfInterface))
            {
                throw new KeyNotFoundException($"Type {typeOfInterface.Name} is not registered");
            }
            return bindings[typeOfInterface];
        }
    }
}
