using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace CToast
{

    public static class ReflectionHelper
    {
        private static List<Assembly> assemblies;
        private static IEnumerable<Assembly> Assemblies
        {
            get
            {
                if (assemblies == null)
                {                   
                    assemblies = new List<Assembly>();
                    assemblies.Add(Assembly.GetExecutingAssembly());
                }

                return assemblies;
            }
        }

        static ReflectionHelper()
        {

        }

        /// <summary>
        /// Creates an object from the subclass decorated with the given attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="ATTR"></typeparam>
        /// <param name="condition"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TObject CreateObjectByAttribute<TObject, TAttribute>(Predicate<TAttribute> condition, params object[] args)
            where TObject : class
            where TAttribute : Attribute
        {
            foreach (Type t in ReflectionHelper.GetSubtypes(typeof(TObject)))
            {
                TAttribute s = t.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault() as TAttribute;
                if (s != null)
                {
                    if (condition(s))
                    {
                        return ReflectionHelper.InvokeObjectConstructor(t, args) as TObject;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the first attribute of the given property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static T GetPropertyAttribute<T>(object obj, String propertyName) where T : Attribute
        {
            return (T)obj.GetType().GetProperty(propertyName).GetCustomAttributes(typeof(T), false).FirstOrDefault();
        }


        /// <summary>
        /// Returns the first attribute of the given class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classType"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(MemberInfo info) where T : Attribute
        {
            foreach (object attr in info.GetCustomAttributes(typeof(T), false))
            {
                T typedAttr = attr as T;
                if (typedAttr != null)
                    return typedAttr;
            }

            return default(T);
        }

        /// <summary>
        /// Returns all instances of the given attribute used in any class in the assembly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetUsedAttributes<T>(Predicate<T> condition) where T : Attribute
        {
            List<T> attributes = new List<T>();
            try
            {
                foreach (var assembly in Assemblies)
                {
                    foreach (var t in assembly.GetTypes())
                    {
                        T attr = GetAttribute<T>(t);
                        if (attr != null)
                        {
                            if (condition(attr))
                                attributes.Add(attr);
                        }
                    }
                }
            }
            catch
            {
            }

            return attributes;
        }


        public static void SetPropertyByName(object obj, String fieldName, object value)
        {
            Type t = obj.GetType();
            foreach (var prop in t.GetProperties())
            {
                if (prop.Name.Equals(fieldName))
                {
                    prop.SetValue(obj, value, null);
                    return;
                }
            }
        }

        public static object GetPropertyByName(object obj, String fieldName)
        {
            Type t = obj.GetType();
            foreach (var prop in t.GetProperties())
            {
                if (prop.Name.Equals(fieldName))
                {
                    return prop.GetValue(obj, null);
                }
            }

            return null;
        }


        public static IEnumerable<PropertyInfo> GetPropertiesByAttribute<TAttribute>(object myObject) where TAttribute : Attribute
        {
            foreach (var property in myObject.GetType().GetProperties())
            {
                TAttribute attribute = property.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault() as TAttribute;
                if (attribute != null)
                {
                    yield return property;
                }
            }
        }

        /// <summary>
        /// Returns the value of the property that has the given attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="O"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static TObject GetPropertyValueByAttribute<TAttribute, TObject>(object obj, Predicate<TAttribute> condition) where TAttribute : Attribute
        {
            Type t = obj.GetType();
            foreach (var prop in t.GetProperties())
            {
                TAttribute a = prop.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault() as TAttribute;
                if (a != null)
                {
                    return (TObject)prop.GetValue(obj, null);
                }
            }

            return default(TObject);
        }


        public static void SetPropertyValueByAttribute<TAttribute, TObject>(object obj, Predicate<TAttribute> condition, TObject value) where TAttribute : Attribute
        {
            Type t = obj.GetType();
            foreach (var prop in t.GetProperties())
            {
                TAttribute a = prop.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault() as TAttribute;
                if (a != null)
                {
                    prop.SetValue(obj, value, null);
                    return;
                }
            }
        }


        public static object GetPropertyByReturnType(object obj, Type returnType)
        {
            Type t = obj.GetType();
            foreach (var prop in t.GetProperties())
            {
                if (prop.PropertyType.Equals(returnType))
                {
                    return prop.GetValue(obj, null);
                }
            }

            return null;
        }

        public static object InvokeObjectConstructor(Type objectType, params object[] ctorArgs)
        {
            foreach (var c in objectType.GetConstructors())
            {
                try
                {
                    object o = c.Invoke(ctorArgs);
                    if (o != null)
                        return o;
                }
                catch (MemberAccessException)
                {
                }
                catch (ArgumentException)
                {
                }
                catch (TargetParameterCountException)
                {
                }
            }

            return null;
        }

        public static object InvokeMethodByAttribute(Type objType, object obj, Predicate<Attribute> comparer, params object[] args)
        {
            Type t = objType;
            foreach (var method in t.GetMethods())
            {
                foreach (Attribute attr in method.GetCustomAttributes(false))
                {
                    if (comparer(attr))
                    {
                        int numParams = method.GetParameters().Length;
                        if (numParams > 0)
                            return method.Invoke(obj, args.Take(numParams).ToArray());
                        else
                            return method.Invoke(obj, null);
                    }
                }
            }

            return null;
        }

        private static Dictionary<string, Type[]> mSubTypeCache = new Dictionary<string, Type[]>();
        public static IEnumerable<Type> GetSubtypes(Type baseType)
        {
            Type[] subTypes;
            if (mSubTypeCache.TryGetValue(baseType.FullName, out subTypes))
                return subTypes;

            subTypes = GetSubTypes2(baseType).ToArray();
            mSubTypeCache.Add(baseType.FullName, subTypes);
            return subTypes;
        }

        private static IEnumerable<Type> GetSubTypes2(Type baseType)
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var t in assembly.GetTypes().Where(p => baseType.IsAssignableFrom(p) && p != baseType))
                    yield return t;
            }
        }

        /// <summary>
        /// Finds a method with the same name as the calling method, but with the specific arguement type.
        /// 
        /// Wish c# did this already!
        /// 
        /// May be slow, best used for editors and not for the actual game.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dynamicArg"></param>
        /// <param name="otherArgs"></param>
        /// <returns></returns>
        public static T DynamicDispatch<T>(object callingObject, params object[] args)
        {
            return DynamicDispatch<T>(callingObject.GetType(), callingObject, args);
        }

        public static T DynamicDispatchStatic<T>(Type callingType, params object[] args)
        {
            return DynamicDispatch<T>(callingType, null,  args);
        }

        private static T DynamicDispatch<T>(Type callingType, object callingObject, params object[] args)
        {
            StackFrame frame = new StackTrace().GetFrame(2);
            string methodName = frame.GetMethod().Name;
            Type dynamicType = args[0].GetType();

            return (T)InvokeMethodByArgumentType(callingType, callingObject, methodName, dynamicType, args);
        }

        private static object InvokeMethodByArgumentType(Type callingObjectType, object callingObject, String methodName, Type argType, params object[] args)
        {
            foreach (var method in callingObjectType.GetMethods())
            {
                if (method.Name.Equals(methodName))
                {
                    foreach (var pInfo in method.GetParameters())
                    {
                        if (pInfo.ParameterType.Equals(argType))
                            return method.Invoke(callingObject, args);
                    }
                }
            }

            return null;
        }
    }
}
