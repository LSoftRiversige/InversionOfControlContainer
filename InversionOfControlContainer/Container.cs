using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

namespace InversionOfControlContainer
{
    public class Container : IContainer
    {
        private readonly Dictionary<Type, ClassDescription> bindings;
        private Dictionary<Type, object> singletonObjects;

        //public -------------------------------------------------------------

        public Dictionary<Type, ClassDescription> Bindings => bindings;

        public Container()
        {
            bindings = new Dictionary<Type, ClassDescription>();
        }

        public ClassDescription Bind<TInterface, TClass>()
        {
            return Bind(typeof(TInterface), typeof(TClass));
        }

        public ClassDescription Bind(Type interfaceType, Type classType)
        {
            CheckBindParams(interfaceType, classType);
            ClassDescription result = new ClassDescription() { ObjectType = classType };
            Bindings[interfaceType] = result;
            CheckStackOverflow(classType, new HashSet<Type>());
            return result;
        }

        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }
                
        public T GetSingltone<T>()
        {
            return (T)Get(typeof(T), isSingltone: true);
        }

        public Lazy<T> GetLazy<T>()
        {
            return new Lazy<T>(() => Get<T>());
        }

        //private---------------------------------------------------------------

        public object Get(Type type, bool isSingltone = false)
        {
            object[] constructorParams = null;

            IClassDescription theClass = FindClassDescription(type);

            constructorParams = InjectParametersToConstructor(theClass);

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

            obj = CreateObject(theClass.ObjectType, constructorParams);

            if (isSingltone)
            {
                AddSingletoneObject(type, obj);
            }

            theClass.InjectPropertiesToObject(obj);

            return obj;
        }

        private void CheckBindParams(Type interfaceType, Type classType)
        {
            if (classType.IsInterface)
            {
                throw new InvalidOperationException($"'{GetName(classType)}' cannot be an interface");
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

        private void CheckStackOverflow(Type classType, HashSet<Type> usedTypes)
        {
            if (!usedTypes.Add(classType))
            {
                throw new StackOverflowException($"Circular constructor parameter reference for class '{classType.Name}'");
            }
            ConstructorInfo constructor = FindConstructorWithMininumParameters(classType);
            ParameterInfo[] paramArray = constructor.GetParameters();
            foreach (var p in paramArray)
            {
                if (p.ParameterType.IsClass)
                {
                    CheckStackOverflow(p.ParameterType, usedTypes);
                }
                else if (p.ParameterType.IsInterface)
                {
                    if (Bindings.TryGetValue(p.ParameterType, out ClassDescription classDescription))
                    {
                        CheckStackOverflow(classDescription.ObjectType, usedTypes);
                    }
                }
            }
        }

        private static string GetName(Type t)
        {
            return t.Name;
        }
        
        private object CreateObject(Type type, object[] constructorParams)
        {
            object obj;
            
            bool noParams = constructorParams == null;
            if (noParams)
            {
                obj = Activator.CreateInstance(type);               
            }
            else
            {
                obj = Activator.CreateInstance(type, constructorParams);
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

        private ClassDescription AddClassDescription(Type t)
        {
            ClassDescription foundClass = new ClassDescription() { ObjectType = t };
            bindings[t] = foundClass;
            return foundClass;
        }

        private object[] InjectParametersToConstructor(IClassDescription theClass)
        {
            ParameterInfo[] parameters = DefineConstructorParams(theClass);
            return GetParamValues(parameters, theClass);
        }

        private static ParameterInfo[] DefineConstructorParams(IClassDescription theClass)
        {
            ConstructorInfo constructor = FindConstructorWithMininumParameters(theClass.ObjectType);
            ParameterInfo[] parameters = constructor.GetParameters();
            return parameters;
        }

        private static ConstructorInfo FindConstructorWithMininumParameters(Type classType)
        {
            return classType.GetConstructors().OrderBy(c => c.GetParameters().Count()).First();
        }

        private object[] GetParamValues(ParameterInfo[] parameters, IClassDescription theClass)
        {
            object[] result = new object[parameters.Length];

            int i = 0;
            foreach (ParameterInfo p in parameters)
            {
                result[i++] = DefineParamValue(theClass, p);
            }
            return result;
        }

        private object DefineParamValue(IClassDescription theClass, ParameterInfo p)
        {
            Type typeOfParam = p.ParameterType;
            TypeCode typeCode = Type.GetTypeCode(typeOfParam);
            bool isObject = typeCode == TypeCode.Object;
            if (isObject)
            {
                return Get(typeOfParam);
            }
            else
            {
                return theClass.GetValueByName(p.Name);
            }
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
    }
}
