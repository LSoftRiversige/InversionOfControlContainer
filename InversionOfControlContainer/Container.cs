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
            if (interfaceType == classType)
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
            constructorParams = InjectConstructorParams(theClass, stack);

            object obj = CreateObject(theClass.ObjectType, constructorParams, isSingltone);

            InjectPropertiesTo(obj, theClass);

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
                if (obj != null)
                {
                    return obj;
                }
            }
            if (constructorParams == null)
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
            if (singletonObjects == null)
            {
                singletonObjects = new Dictionary<Type, object>();
            }
            singletonObjects.Add(type, obj);
        }

        private object FindSingletoneObject(Type type)
        {
            if (singletonObjects == null)
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
            IClassDescription foundClass = bindings.FirstOrDefault(p => p.Value.ObjectType.Equals(t)).Value;
            if (foundClass == null)
            {
                foundClass = new ClassDescription() { ObjectType = t };
                bindings[t] = foundClass;
            }
            return foundClass;
        }

        private void InjectPropertiesTo(object obj, IClassDescription theClass)
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

        private object[] InjectConstructorParams(IClassDescription theClass, HashSet<Type> stack)
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
                throw new InvalidOperationException($"Circular constructor parameter reference for class '{typeOfParam.Name}'");
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

        private IClassDescription DescriptionByInterface(Type typeOfInterface)
        {
            if (!bindings.ContainsKey(typeOfInterface))
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
