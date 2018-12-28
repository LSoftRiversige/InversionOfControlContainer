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
        private Dictionary<Type, object> singletonObjects;

        public Dictionary<Type, IClassDescription> Bindings => bindings;

        public Container()
        {
            bindings = new Dictionary<Type, IClassDescription>();
        }

        public IContainer Bind<TInterface, TClass>()
        {
            return Bind(typeof(TInterface), typeof(TClass));
        }

        private void CheckBindParams(Type interfaceType, Type classType)
        {
            if (interfaceType.IsClass)
            {
                throw new InvalidOperationException($"'{GetName(interfaceType)}' must be an interface");
            }
            bool same = interfaceType == classType;
            if (same)
            {
                throw new InvalidOperationException($"'{GetName(interfaceType)}' and '{GetName(classType)}' types must be different");
            }
            if (Bindings.ContainsKey(interfaceType))
            {
                throw new InvalidOperationException($"Duplicate register of binding '{GetName(interfaceType)}' => '{GetName(classType)}'");
            }
        }

        private static string GetName(Type t)
        {
            return t.Name;
        }

        public T GetSingltone<T>()
        {
            return (T)Get(typeof(T), isSingltone: true);
        }

        public IContainer WithConstructorArgument(string paramName, object paramValue)
        {
            IClassDescription classDescription = GetLastBindingClass("WithConstructorArgument");
            classDescription.WithConstructorArgument(paramName, paramValue);
            return this;
        }

        public IContainer Bind(Type interfaceType, Type classType)
        {
            CheckBindParams(interfaceType, classType);
            lastBinding = interfaceType;
            Bindings[lastBinding] = new ClassDescription() { ObjectType = classType };
            return this;
        }

        public IContainer WithPropertyValue(string name, object value)
        {
            IClassDescription classDescription = GetLastBindingClass("WithPropertyValue");
            classDescription.WithPropertyValue(name, value);
            return this;
        }

        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }

        private object Get(Type type, HashSet<Type> stack = null, bool isSingltone = false)
        {
            object[] constructorParams = null;

            IClassDescription theClass = FindClassDescription(type);

            if (stack == null)
            {
                stack = new HashSet<Type> { type };
            }
            constructorParams = InjectParametersToConstructor(theClass, stack);

            object obj = CreateObject(theClass.ObjectType, constructorParams, isSingltone);

            InjectPropertiesToObject(obj, theClass);

            return obj;
        }

        private IClassDescription GetLastBindingClass(string methodName)
        {
            CheckLastBinding(methodName);
            IClassDescription classDescription = Bindings[lastBinding];
            return classDescription;
        }

        private void CheckLastBinding(string methodName)
        {
            if (lastBinding == null)
            {
                throw new InvalidOperationException($"Method '{methodName}' must called after 'Bind'");
            }
        }

        private object CreateObject(Type type, object[] constructorParams, bool isSingltone)
        {
            object obj;
            if (isSingltone)
            {
                obj = FindSingletoneObject(type);
                bool singletoneFound = obj != null;
                if (singletoneFound)
                {
                    return obj;
                }
            }
            bool noParams = constructorParams == null;
            if (noParams)
            {
                obj = Activator.CreateInstance(type);
            }
            else
            {
                obj = Activator.CreateInstance(type, constructorParams);
            }
            if (isSingltone)
            {
                AddSingletoneObject(type, obj);
            }
            return obj;
        }

        private void AddSingletoneObject(Type type, object obj)
        {
            TryToCreateSingletoneBuffer();
            singletonObjects.Add(type, obj);
        }

        private bool TryToCreateSingletoneBuffer()
        {
            if (singletonObjects == null)
            {
                singletonObjects = new Dictionary<Type, object>();
                return true;
            }
            return false;
        }

        private object FindSingletoneObject(Type type)
        {
            bool notCreated = singletonObjects == null;
            if (notCreated)
            {
                return null;
            }
            singletonObjects.TryGetValue(type, out object obj);
            return obj;
        }

        private IClassDescription FindClassDescription(Type type)
        {
            if (type.IsClass)
            {
                return DescriptionByClass(type);
            }
            if (type.IsInterface)
            {
                return DescriptionByInterface(type);
            }
            return null;
        }

        private IClassDescription DescriptionByClass(Type t)
        {
            IClassDescription foundClass = FindBindingByClassType(t);
            if (foundClass == null)
            {
                foundClass = AddClassDescription(t);
            }
            return foundClass;
        }

        private IClassDescription FindBindingByClassType(Type t)
        {
            return bindings.FirstOrDefault(p => p.Value.ObjectType.Equals(t)).Value;
        }

        private IClassDescription AddClassDescription(Type t)
        {
            IClassDescription foundClass = new ClassDescription() { ObjectType = t };
            bindings[t] = foundClass;
            return foundClass;
        }

        private void InjectPropertiesToObject(object obj, IClassDescription theClass)
        {
            if (theClass != null)
            {
                PropertyInfo[] properties = theClass.ObjectType.GetProperties();
                foreach (var prop in properties)
                {
                    theClass.TryInjectProperty(prop, obj);
                }
            }
        }

        private object[] InjectParametersToConstructor(IClassDescription theClass, HashSet<Type> stack)
        {
            ConstructorInfo constructor = GetConstructorWithMinimumParams(theClass);
            ParameterInfo[] parameters = constructor.GetParameters();
            return GetParamValues(parameters, theClass, stack);
        }

        private static ConstructorInfo GetConstructorWithMinimumParams(IClassDescription theClass)
        {
            return theClass.ObjectType.GetConstructors().OrderBy(c => c.GetParameters().Count()).First();
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
            bool isObject = typeCode == TypeCode.Object;
            if (isObject)
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
            bool inStack = stack.Contains(typeOfParam);
            if (inStack)
            {
                throw new InvalidOperationException($"Circular constructor parameter reference for class '{typeOfParam.Name}'");
            }
        }

        private object GetValueByName(string name, IClassDescription classDescription)
        {
            bool found = classDescription.ConstructorParamValues.ContainsKey(name);
            if (!found)
            {
                throw new KeyNotFoundException($"Parameter {name} not found in value list");
            }
            return classDescription.ConstructorParamValues[name];
        }

        private IClassDescription DescriptionByInterface(Type typeOfInterface)
        {
            bool found = bindings.ContainsKey(typeOfInterface);
            if (!found)
            {
                throw new KeyNotFoundException($"Type {typeOfInterface.Name} is not registered");
            }
            return bindings[typeOfInterface];
        }

        public Lazy<T> GetLazy<T>()
        {
            return new Lazy<T>(() => Get<T>());
        }
    }
}
