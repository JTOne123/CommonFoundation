﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Beyova
{
    public static partial class ReflectionExtension
    {
        #region PropertyInfoEquailtyComparer

        /// <summary>
        /// Class PropertyInfoEquailtyComparer
        /// </summary>
        private class PropertyInfoEquailtyComparer : IEqualityComparer<PropertyInfo>
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <param name="x">The first object of type <see cref="PropertyInfo"/> to compare.</param>
            /// <param name="y">The second object of type <see cref="PropertyInfo"/> to compare.</param>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            public bool Equals(PropertyInfo x, PropertyInfo y)
            {
                if (x == null || y == null)
                {
                    return false;
                }

                return x.Name.Equals(y.Name) && x.PropertyType == y.PropertyType;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
            /// </returns>
            public int GetHashCode(PropertyInfo obj)
            {
                return obj?.Name?.GetHashCode() ?? 0 + obj?.PropertyType?.GetHashCode() ?? 0;
            }
        }

        #endregion PropertyInfoEquailtyComparer

        /// <summary>
        /// The generic regex
        /// </summary>
        private static Regex genericClassRegex = new Regex(@"(?<TypeName>([\w\.]+`([0-9]+)))\[(?<GenericTypeName>(([\w\s,\.\[\],`]+)))\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// The generic code look class regex
        /// </summary>
        private static Regex genericCodeLookClassRegex = new Regex(@"(?<TypeName>([\w\.]+))\<(?<GenericTypeName>(([\w\s,\.\[\],`]+)))\>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Gets the application domain assemblies.
        /// </summary>
        /// <param name="appDomain">The application domain.</param>
        /// <returns>IEnumerable{Assembly}.</returns>
        public static ICollection<Assembly> GetAppDomainAssemblies(AppDomain appDomain = null)
        {
            return (appDomain ?? AppDomain.CurrentDomain).GetAssemblies();
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="ignoreCase">The ignore case.</param>
        /// <param name="throwException">The throw exception.</param>
        /// <returns>System.Type.</returns>
        public static Type GetType(this Assembly assembly, string typeName, bool ignoreCase, bool throwException = true)
        {
            Type type = null;

            if (assembly != null && !string.IsNullOrWhiteSpace(typeName))
            {
                try
                {
                    foreach (var one in assembly.GetTypes())
                    {
                        if (one.Name.Equals(typeName, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
                            || one.FullName.Equals(typeName, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                        {
                            type = one;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (throwException)
                    {
                        throw ex.Handle(new { Assembly = assembly?.FullName, TypeName = typeName, IgnoreCase = ignoreCase });
                    }
                }
            }

            return type;
        }

        /// <summary>
        /// Tries the type of the get.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="ignoreCase">The ignore case.</param>
        /// <param name="type">The type.</param>
        /// <returns>System.Boolean.</returns>
        public static bool TryGetType(this Assembly assembly, string typeName, bool ignoreCase, out Type type)
        {
            type = GetType(assembly, typeName, ignoreCase, false);
            return type != null;
        }

        /// <summary>
        /// Gets the assembly public key token.
        /// </summary>
        /// <param name="anyAssembly">Any assembly.</param>
        /// <returns>System.String.</returns>
        public static string GetAssemblyPublicKeyToken(this Assembly anyAssembly)
        {
            return anyAssembly != null ? anyAssembly.GetName().GetPublicKeyToken().ToHex() : string.Empty;
        }

        /// <summary>
        /// Determines whether [is system assembly] [the specified assembly].
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>System.Boolean.</returns>
        public static bool IsSystemAssembly(this Assembly assembly)
        {
            var info = assembly?.GetName();
            return info == null ? false : info.IsSystemAssembly();
        }

        /// <summary>
        /// Determines whether [is system assembly] [the specified assembly name].
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>System.Boolean.</returns>
        internal static bool IsSystemAssembly(this AssemblyName assemblyName)
        {
            return assemblyName != null
                && (assemblyName.Name.StartsWith("Microsoft.")
                    || assemblyName.Name.StartsWith("System.")
                    || assemblyName.Name.Equals("system", StringComparison.OrdinalIgnoreCase)
                    || assemblyName.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase));
        }

        #region Property Info

        /// <summary>
        /// Gets the public properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns></returns>
        public static List<PropertyInfo> GetPublicProperties(this Type type, bool inherit)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.FlattenHierarchy, inherit);
        }

        /// <summary>
        /// Gets the public property.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns></returns>
        public static PropertyInfo GetPublicProperty(this Type type, string name, bool inherit)
        {
            return string.IsNullOrWhiteSpace(name) ? null : GetPublicProperties(type, inherit).Where(p => p.Name.Equals(name)).FirstOrDefault();
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static List<PropertyInfo> GetProperties(this Type type, BindingFlags bindingFlags, bool inherit, Func<PropertyInfo, bool> filter = null)
        {
            if (type != null)
            {
                if (inherit)
                {
                    var properties = new HashSet<PropertyInfo>(new PropertyInfoEquailtyComparer());
                    InternalFillProperties(properties, type, bindingFlags, filter);
                    return properties.ToList();
                }
                else
                {
                    var tmpResult = type.GetProperties(bindingFlags);
                    return (filter == null ? tmpResult : tmpResult.Where(filter)).ToList();
                }
            }

            return new List<PropertyInfo>();
        }

        /// <summary>
        /// Internals the fill properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="type">The type.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <param name="filter">The filter.</param>
        private static void InternalFillProperties(HashSet<PropertyInfo> properties, Type type, BindingFlags bindingFlags, Func<PropertyInfo, bool> filter = null)
        {
            if (properties != null && type != null)
            {
                if (type.BaseType != null && type.BaseType != typeof(object))
                {
                    InternalFillProperties(properties, type.BaseType, bindingFlags, filter);
                }

                foreach (var interfaceItem in type.GetInterfaces())
                {
                    InternalFillProperties(properties, interfaceItem, bindingFlags, filter);
                }

                foreach (var item in type.GetProperties(bindingFlags))
                {
                    if (filter == null || filter(item))
                    {
                        properties.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the specified property information is override.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns><c>true</c> if the specified property information is override; otherwise, <c>false</c>.</returns>
        public static bool IsOverride(this PropertyInfo propertyInfo)
        {
            if (propertyInfo != null)
            {
                var getMethod = propertyInfo.GetGetMethod();
                if ((getMethod.Attributes & MethodAttributes.Virtual) != 0 &&
                    (getMethod.Attributes & MethodAttributes.NewSlot) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified property information is virtual.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns><c>true</c> if the specified property information is virtual; otherwise, <c>false</c>.</returns>
        public static bool IsVirtual(this PropertyInfo propertyInfo)
        {
            if (propertyInfo != null)
            {
                var getMethod = propertyInfo.GetGetMethod();
                if ((getMethod.Attributes & MethodAttributes.Virtual) != 0 &&
                    (getMethod.Attributes & MethodAttributes.NewSlot) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion Property Info

        /// <summary>
        /// Gets the actual affected properties. This method helps to get properties without duplication.
        /// </summary>
        /// <param name="anyType">Any type.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <returns>List&lt;PropertyInfo&gt;.</returns>
        public static List<PropertyInfo> GetActualAffectedProperties(this Type anyType, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
        {
            var returnedProperties = new List<PropertyInfo>();

            if (anyType != null)
            {
                HashSet<string> propertyNameList = new HashSet<string>();

                foreach (var one in anyType.GetProperties(bindingFlags))
                {
                    if (!propertyNameList.Contains(one.Name))
                    {
                        returnedProperties.Add(one);
                        propertyNameList.Add(one.Name);
                    }
                }
            }

            return returnedProperties;
        }

        /// <summary>
        /// Gets the actual affected fields. This method helps to get fields without duplication.
        /// </summary>
        /// <param name="anyType">Any type.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <returns>List&lt;FieldInfo&gt;.</returns>
        public static List<FieldInfo> GetActualAffectedFields(this Type anyType, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField)
        {
            var returnedFields = new List<FieldInfo>();
            if (anyType != null)
            {
                HashSet<string> fieldNameList = new HashSet<string>();

                foreach (var one in anyType.GetFields(bindingFlags))
                {
                    if (!fieldNameList.Contains(one.Name))
                    {
                        returnedFields.Add(one);
                        fieldNameList.Add(one.Name);
                    }
                }
            }

            return returnedFields;
        }

        #region Create instance

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>System.Object.</returns>
        public static object CreateInstance(this Type type, object[] arguments = null)
        {
            if (type != null)
            {
                return Activator.CreateInstance(type, arguments);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>System.Object.</returns>
        public static object CreateInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }

        /// <summary>
        /// Creates the generic instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="genericTypes">The generic types.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>System.Object.</returns>
        public static object CreateGenericInstance(this Type type, Type[] genericTypes, object[] arguments = null)
        {
            if (type != null)
            {
                var genericType = type.MakeGenericType(genericTypes);
                return Activator.CreateInstance(genericType, arguments);
            }

            return null;
        }

        #endregion Create instance

        #region Static utility methods

        /// <summary>
        /// Gets the assembly file version.
        /// </summary>
        /// <param name="assemblyObject">The assembly object.</param>
        /// <returns>System.String.</returns>
        public static string GetAssemblyFileVersion(this Assembly assemblyObject)
        {
            if (assemblyObject != null)
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assemblyObject.Location);
                return fvi.FileVersion;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the assembly component version.
        /// </summary>
        /// <param name="assemblyObject">The assembly object.</param>
        /// <returns></returns>
        public static string GetAssemblyComponentVersion(this Assembly assemblyObject)
        {
            if (assemblyObject != null)
            {
                var beyovaComponentAttribute = assemblyObject.GetCustomAttribute<BeyovaComponentAttribute>();

                if (beyovaComponentAttribute != null)
                {
                    return beyovaComponentAttribute.UnderlyingObject.Version;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the assembly version.
        /// </summary>
        /// <param name="assemblyObject">The assembly object.</param>
        /// <returns>Version.</returns>
        public static Version GetAssemblyVersion(this Assembly assemblyObject)
        {
            return assemblyObject?.GetName()?.Version;
        }

        /// <summary>
        /// Gets the name of the generic type. e.g.: System.Collections.Generic.List`1[Beyova.ServiceCredential]
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="isCodeLook">The is code look.</param>
        /// <param name="genericTypeNames">The generic type names.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <returns>
        ///   <c>true</c> if succeed to get, <c>false</c> otherwise.</returns>
        internal static bool GetGenericTypeName(string fullName, bool isCodeLook, out string[] genericTypeNames, out string typeName)
        {
            typeName = string.Empty;
            genericTypeNames = new string[] { };

            var match = (isCodeLook ? genericCodeLookClassRegex : genericClassRegex).Match(fullName);
            if (match.Success)
            {
                genericTypeNames = match.Result("${GenericTypeName}").Split(new char[] { StringConstants.CommaChar }, StringSplitOptions.RemoveEmptyEntries);
                typeName = match.Result("${TypeName}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all interfaces.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="includingItself">if set to <c>true</c> [including itself].</param>
        /// <returns></returns>
        public static Type[] GetAllInterfaces(this Type type, bool includingItself = false)
        {
            HashSet<Type> result = new HashSet<Type>();

            if (type != null)
            {
                if (includingItself && type.IsInterface)
                {
                    result.Add(type);
                }

                result.HoldFlatItems(type, x => x.GetInterfaces());
            }

            return result.ToArray();
        }

        /// <summary>
        /// Fills the interfaces requires attribute.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="type">The type.</param>
        /// <param name="attribute">The attribute.</param>
        private static void FillInterfacesRequiresAttribute<TAttribute>(Dictionary<Type, TAttribute> container, Type type, TAttribute attribute)
            where TAttribute : Attribute
        {
            if (container != null && type != null)
            {
                foreach (var item in type.GetInterfaces())
                {
                    var inheritAttribute = attribute ?? item.GetCustomAttribute<TAttribute>(true);

                    if (attribute != null)
                    {
                        container.Add(item, attribute);
                        FillInterfacesRequiresAttribute(container, item, inheritAttribute);
                    }
                }
            }
        }

        /// <summary>
        /// Gets all interfaces requires attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="type">The type.</param>
        /// <param name="includingItself">if set to <c>true</c> [including itself].</param>
        /// <param name="inheritAttribute">if set to <c>true</c> [inherit attribute]. If true, then in inherit chain, only top level needs attribute.</param>
        /// <returns></returns>
        public static Dictionary<Type, TAttribute> GetAllInterfacesRequiresAttribute<TAttribute>(this Type type, bool includingItself = false, bool inheritAttribute = false)
            where TAttribute : Attribute
        {
            var result = new Dictionary<Type, TAttribute>();

            if (type != null)
            {
                var attribute = type.GetCustomAttribute<TAttribute>(true);

                if (includingItself && type.IsInterface && attribute != null)
                {
                    result.Merge(type, attribute, false);
                }

                FillInterfacesRequiresAttribute(result, type, inheritAttribute ? attribute : null);
            }

            return result;
        }

        /// <summary>
        /// Gets the type smartly. It would find type in all related assemblies. And even if type name is generic based, it can still give right result.
        /// e.g.: System.Collections.Generic.List`1[Beyova.ServiceCredential]
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="isCodeLook">The is code look.</param>
        /// <returns>Type.</returns>
        public static Type SmartGetType(string typeName, bool isCodeLook = false)
        {
            try
            {
                typeName.CheckEmptyString(nameof(typeName));
                typeName = typeName.Trim();

                string baseTypeName;
                string[] genericTypeNames;
                if (GetGenericTypeName(typeName, isCodeLook, out genericTypeNames, out baseTypeName))
                {
                    if (!baseTypeName.Contains('`'))
                    {
                        baseTypeName = string.Format("{0}`{1}", baseTypeName, genericTypeNames.Length);
                    }

                    //NOTE: here, baseTypeName would be assembly based name, so force to use false as isCodeLook.
                    var baseType = SmartGetType(baseTypeName, false);

                    if (baseType == typeof(Nullable))
                    {
                        baseType = typeof(Nullable<>);
                    }

                    return baseType.MakeGenericType((from one in genericTypeNames select SmartGetType(one, isCodeLook)).ToArray());
                }
                else
                {
                    var typeResult = Type.GetType(typeName);

                    if (typeResult == null)
                    {
                        var assemblies = GetAppDomainAssemblies();

                        if (assemblies != null)
                        {
                            foreach (var one in assemblies)
                            {
                                if (!one.IsSystemAssembly() && one.TryGetType(typeName, false, out typeResult))
                                {
                                    break;
                                }
                            }
                        }
                    }

                    return typeResult;
                }
            }
            catch (Exception ex)
            {
                var typeLoadException = ex as ReflectionTypeLoadException;

                if (typeLoadException != null && typeLoadException.LoaderExceptions.Any())
                {
                    throw typeLoadException.LoaderExceptions.First().Handle(typeName);
                }
                else
                {
                    throw ex.Handle(typeName);
                }
            }
        }

        /// <summary>
        /// Matches the interface method.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="attributeInherit">if set to <c>true</c> [attribute inherit].</param>
        /// <returns><c>true</c> if matches interface method, <c>false</c> otherwise.</returns>
        private static bool MatchInterfaceMethod(MethodInfo methodInfo, Type attributeType, bool attributeInherit)
        {
            return (attributeType == null || methodInfo.GetCustomAttribute(attributeType, attributeInherit) != null);
        }

        /// <summary>
        /// Gets the interface method. This method would try to find methods in inheritance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="attributeInherit">if set to <c>true</c> [attribute inherit].</param>
        /// <returns>MethodInfo.</returns>
        public static MethodInfo GetInterfaceMethod(this Type type, string name, bool ignoreCase = false, Type attributeType = null, bool attributeInherit = true)
        {
            if (!string.IsNullOrWhiteSpace(name) && type != null)
            {
                if (type.IsInterface)
                {
                    foreach (var item in type.GetMethods())
                    {
                        if (item.Name.Equals(name, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
                            && MatchInterfaceMethod(item, attributeType, attributeInherit))
                        {
                            return item;
                        }
                    }
                }

                foreach (var one in type.GetInterfaces())
                {
                    foreach (var item in one.GetMethods())
                    {
                        if (item.Name.Equals(name, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
                            && MatchInterfaceMethod(item, attributeType, attributeInherit))
                        {
                            return item;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the interface methods. This method would try to find methods in inheritance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="attributeInherit">if set to <c>true</c> [attribute inherit].</param>
        /// <returns>List&lt;MethodInfo&gt;.</returns>
        public static List<MethodInfo> GetInterfaceMethods(this Type type, Type attributeType = null, bool attributeInherit = false)
        {
            HashSet<MethodInfo> result = new HashSet<MethodInfo>();

            if (type != null)
            {
                if (type.IsInterface)
                {
                    foreach (var item in type.GetMethods())
                    {
                        if (MatchInterfaceMethod(item, attributeType, attributeInherit))
                        {
                            result.Add(item);
                        }
                    }
                }

                foreach (var one in type.GetInterfaces())
                {
                    foreach (var item in one.GetMethods())
                    {
                        if (MatchInterfaceMethod(item, attributeType, attributeInherit))
                        {
                            result.Add(item);
                        }
                    }
                }
            }

            return result.ToList();
        }

        /// <summary>
        /// Gets the type of the generic.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="genericTypeNames">The generic type names.</param>
        /// <returns>Type.</returns>
        public static Type GetGenericType(string typeName, string[] genericTypeNames)
        {
            try
            {
                typeName.CheckEmptyString(nameof(typeName));

                var containerType = SmartGetType(typeName);
                return (containerType != null) ? containerType.MakeGenericType(genericTypeNames.Select(one => SmartGetType(one)).ToArray()) : null;
            }
            catch (Exception ex)
            {
                throw ex.Handle(new { typeName, genericTypeNames });
            }
        }

        #endregion Static utility methods

        #region Extensions

        /// <summary>
        /// Determines whether this type is <see cref="IBaseObject"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if type is <see cref="IBaseObject"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBaseObject(this Type type)
        {
            return type != null && typeof(IBaseObject).IsAssignableFrom(type);
        }

        /// <summary>
        /// Determines whether this type is <see cref="ISimpleBaseObject"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if type is <see cref="ISimpleBaseObject"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSimpleBaseObject(this Type type)
        {
            return type != null && typeof(ISimpleBaseObject).IsAssignableFrom(type);
        }

        /// <summary>
        /// Determines whether this type is <see cref="Nullable"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if type is <see cref="Nullable"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullable(this Type type)
        {
            return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Ensures the type of the underlying.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Type EnsureUnderlyingType(this Type type)
        {
            return (type != null && type.IsNullable()) ? Nullable.GetUnderlyingType(type) : type;
        }

        /// <summary>
        /// Acts the on type with nullable underlying.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static T ActOnTypeWithNullableUnderlying<T>(this Type type, Func<Type, T> action)
        {
            if (type != null && action != null)
            {
                return action(EnsureUnderlyingType(type));
            }

            return default(T);
        }

        /// <summary>
        /// Checks the type with nullable underlying. return action(type) || action(Nullable.GetUnderlyingType(type));
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="action">The action.</param>
        /// <returns>
        /// return action(type) || action(Nullable.GetUnderlyingType(type));
        /// </returns>
        public static bool CheckTypeWithNullableUnderlying(this Type type, Func<Type, bool> action)
        {
            return ActOnTypeWithNullableUnderlying(type, action);
        }

        /// <summary>
        /// Determines whether this instance is enum.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is enum; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEnum(this Type type)
        {
            return CheckTypeWithNullableUnderlying(type, t => t.IsEnum);
        }

        /// <summary>
        /// Determines whether this instance [can be null] the specified object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can be null] the specified object; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanBeNull<T>(this T obj)
        {
            //Smart way copy from stackoverflow and msdn
            // http://stackoverflow.com/questions/374651/how-to-check-if-an-object-is-nullable
            // http://referencesource.microsoft.com/#mscorlib/system/collections/objectmodel/readonlycollection.cs,219

            return default(T) == null;
        }

        /// <summary>
        /// Determines whether this instance [can be null] the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can be null] the specified type; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanBeNull(this Type type)
        {
            return type != null && (type.IsClass || type.IsNullable());
        }

        /// <summary>
        /// Determines whether the specified type is dictionary.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is dictionary; otherwise, <c>false</c>.</returns>
        public static bool IsDictionary(this Type type)
        {
            return type != null && typeof(IDictionary<,>).IsAssignableFrom(type);
        }

        /// <summary>
        /// Determines whether the specified type is collection.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is collection; otherwise, <c>false</c>.</returns>
        public static bool IsCollection(this Type type)
        {
            return type != null && (type.IsArray || typeof(ICollection<>).IsAssignableFrom(type));
        }

        #region Nullable

        /// <summary>
        /// Gets the type of the nullable. If type is nullable, return underlying type. Otherwise return input type.
        /// <remarks>
        /// If specific type is not Nullable, return type itself.
        /// </remarks>
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type.</returns>
        public static Type TryGetNullableType(this Type type)
        {
            return GetNullableType(type) ?? type;
        }

        /// <summary>
        /// Gets the type of the nullable.
        /// <remarks>
        /// If specific type is not Nullable, return null.
        /// </remarks>
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type.</returns>
        public static Type GetNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type);
        }

        #endregion Nullable

        /// <summary>
        /// Determines whether [is simple type] [the specified type]. Like: Guid, string, int32, int64, etc.
        /// If it is Nullable&lt;T&gt;, it would detect T directly.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if [is simple type] [the specified type]; otherwise, <c>false</c>.</returns>
        public static bool IsSimpleType(this Type type)
        {
            return IsPrimitiveType(type);
        }

        /// <summary>
        /// Determines whether [is primitive type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is primitive type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPrimitiveType(this Type type)
        {
            return CheckTypeWithNullableUnderlying(type, InternalIsPrimitiveType);
        }

        /// <summary>
        /// Internals the type of the is primitive.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static bool InternalIsPrimitiveType(Type type)
        {
            return type != null && (type.IsPrimitive
               || typeof(string) == type
               || typeof(Guid) == type
               || typeof(DateTime) == type
               || typeof(decimal) == type
               || typeof(TimeSpan) == type
               || typeof(Uri) == type
               || type.IsEnum);
        }

        /// <summary>
        /// Determines whether [is integer based type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is integer based type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsIntegerBasedType(this Type type)
        {
            return CheckTypeWithNullableUnderlying(type, InternalIsIntegerBasedType);
        }

        /// <summary>
        /// Internals the type of the is integer based.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static bool InternalIsIntegerBasedType(Type type)
        {
            return type != null && (type.IsEnum
                || type.FullName.StartsWith("System.Int")
                || type.FullName.StartsWith("System.UInt"));
        }

        /// <summary>
        /// Determines whether [is numeric primitive type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is numeric primitive type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumericPrimitiveType(this Type type)
        {
            return CheckTypeWithNullableUnderlying(type, InternalIsNumericPrimitiveType);
        }

        /// <summary>
        /// Internals the type of the is numeric primitive.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static bool InternalIsNumericPrimitiveType(Type type)
        {
            return type != null && (type.IsEnum
                   || IsIntegerBasedType(type)
                   || typeof(decimal) == type
                   || typeof(float) == type
                   || typeof(double) == type
                   || typeof(Boolean) == type
                   || typeof(FractionObject) == type);
        }

        /// <summary>
        /// Determines whether the specified method has attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method">The method.</param>
        /// <param name="inherit">The inherit.</param>
        /// <returns><c>true</c> if the specified method has attribute; otherwise, <c>false</c>.</returns>
        public static bool HasAttribute<T>(this MethodInfo method, bool inherit = true) where T : Attribute
        {
            return method != null && method.GetCustomAttribute<T>(inherit) != null;
        }

        /// <summary>
        /// Determines whether the specified method has attribute.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns><c>true</c> if the specified method has attribute; otherwise, <c>false</c>.</returns>
        public static bool HasAttribute(this MethodInfo method, Attribute attribute, bool inherit = true)
        {
            return method != null && method.GetCustomAttribute(attribute.GetType(), inherit) != null;
        }

        /// <summary>
        /// Determines whether the specified type has attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns><c>true</c> if the specified type has attribute; otherwise, <c>false</c>.</returns>
        public static bool HasAttribute<T>(this Type type, bool inherit = true) where T : Attribute
        {
            return type?.GetCustomAttribute<T>(inherit) != null;
        }

        /// <summary>
        /// Determines whether the specified type has attribute.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns><c>true</c> if the specified type has attribute; otherwise, <c>false</c>.</returns>
        public static bool HasAttribute(this Type type, Attribute attribute, bool inherit = true)
        {
            return type?.GetCustomAttribute(attribute.GetType(), inherit) != null;
        }

        /// <summary>
        /// Gets the parameter type from method info.
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        /// <param name="index">The index.</param>
        /// <returns>Type.</returns>
        public static Type GetParameterType(this MethodInfo methodInfo, int index = 0)
        {
            Type result = null;

            if (methodInfo != null)
            {
                var parameters = methodInfo.GetParameters();
                if (parameters != null && index < parameters.Length)
                {
                    result = parameters[index].ParameterType;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="anyObject">Any object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>System.Object.</returns>
        public static object GetPropertyValue(object anyObject, string propertyName)
        {
            object result = null;

            if (anyObject != null && !string.IsNullOrWhiteSpace(propertyName))
            {
                var property = anyObject.GetType().GetProperty(propertyName);

                if (property != null)
                {
                    result = property.GetValue(anyObject);
                }
            }

            return result;
        }

        #endregion Extensions

        /// <summary>
        /// Gets the method information within attribute. As MSDN said, you have to assign either <see cref="BindingFlags.Instance"/> or <see cref="BindingFlags.Static"/> for bindingFlags, so that you can get right result.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the t attribute.</typeparam>
        /// <param name="type">The type.</param>
        /// <param name="inherit">The inherit.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <returns>IEnumerable&lt;MethodInfo&gt;.</returns>
        public static IEnumerable<MethodInfo> GetMethodInfoWithinAttribute<TAttribute>(this Type type, bool inherit = true, BindingFlags bindingFlags = BindingFlags.Default) where TAttribute : Attribute
        {
            List<MethodInfo> result = new List<MethodInfo>();

            if (type != null)
            {
                result.AddRange(from item in type.GetMethods(bindingFlags)
                                where item.HasAttribute<TAttribute>(inherit)
                                select item);

                if (inherit)
                {
                    foreach (var one in type.GetInterfaces())
                    {
                        result.AddRange(GetMethodInfoWithinAttribute<TAttribute>(one, inherit, bindingFlags));
                    }

                    result = result.Distinct().ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified type is void.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is void; otherwise, <c>false</c>.</returns>
        public static bool? IsVoid(this Type type)
        {
            return type == null ? null : (type == typeof(void)) as bool?;
        }

        /// <summary>
        /// Determines whether the specified default value is void.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <returns>
        ///   <c>true</c> if the specified default value is void; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsVoid(this Type type, bool defaultValue)
        {
            return type == null ? defaultValue : (type == typeof(void));
        }

        /// <summary>
        /// Determines whether this instance is asynchronous.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>
        ///   <c>true</c> if the specified method is asynchronous; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAsync(this MethodInfo method)
        {
            return method?.GetCustomAttribute<AsyncStateMachineAttribute>(true) != null;
        }

        /// <summary>
        /// Determines whether this instance is task.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is task; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTask(this Type type)
        {
            return type != null && typeof(Task).IsAssignableFrom(type);
        }

        /// <summary>
        /// Gets the type of the task underlying. If given Task%lt;x%gt;, return x; given Task, return Void, otherwise return null.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Type GetTaskUnderlyingType(this Type type)
        {
            if (type == null || !typeof(Task).IsAssignableFrom(type))
            {
                return null;
            }

            if (type.IsGenericType)
            {
                return type.GetGenericArguments().FirstOrDefault();
            }
            else
            {
                return typeof(void);
            }
        }

        /// <summary>
        /// Gets the generic parameter names.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="genericParameterTypeNames">The generic parameter type names.</param>
        /// <returns>System.Collections.Generic.List&lt;System.String&gt;.</returns>
        internal static List<string> GetGenericParameterNames(this Type type, ICollection<string> genericParameterTypeNames)
        {
            List<string> result = new List<string>();

            if (type != null)
            {
                if (type.IsGenericType)
                {
                    foreach (var one in type.GetGenericArguments())
                    {
                        result.AddRange(GetGenericParameterNames(one, genericParameterTypeNames));
                    }
                }
                else if (type.IsGenericParameter)
                {
                    result.Add(type.Name);
                }
            }

            return result;
        }

        /// <summary>
        /// Equalses the type of the by underplying generic.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        public static bool EqualsByUnderplyingGenericType(this Type type, Type targetType)
        {
            return type != null && targetType != null
                    && type.IsGenericType && targetType.IsGenericType
                    && type.Namespace.Equals(targetType.Namespace)
                    && type.Name.Equals(targetType.Name)
                    && type.GetGenericArguments().Length.Equals(targetType.GetGenericArguments().Length);
        }

        /// <summary>
        /// Gets the constant fields.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="isPublic">if set to <c>true</c> [is public].</param>
        /// <returns></returns>
        public static FieldInfo[] GetConstantFields(this Type type, bool isPublic)
        {
            return type?.GetFields(BindingFlags.Static | (isPublic ? BindingFlags.Public : BindingFlags.NonPublic) | BindingFlags.GetField)?.Where(x => x.IsLiteral)?.ToArray();
        }

        #region Assembly Dependency Chain

        /// <summary>
        /// Class AssemblyDependency.
        /// </summary>
        private class AssemblyDependency
        {
            /// <summary>
            /// Gets or sets the last.
            /// </summary>
            /// <value>The last.</value>
            public AssemblyDependency Last { get; set; }

            /// <summary>
            /// Gets or sets the next.
            /// </summary>
            /// <value>The next.</value>
            public AssemblyDependency Next { get; set; }

            /// <summary>
            /// Gets or sets the assembly.
            /// </summary>
            /// <value>The assembly.</value>
            public Assembly Assembly { get; set; }

            /// <summary>
            /// Gets or sets the level.
            /// </summary>
            /// <value>The level.</value>
            public int Level { get; set; }

            /// <summary>
            /// Gets or sets the referenced count.
            /// </summary>
            /// <value>The referenced count.</value>
            public int ReferencedCount { get; set; }
        }

        /// <summary>
        /// Gets the assembly dependency chain. Descending is for referenced count. So if descending is true, then Json.NET should be ahead of common.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="descending">The descending.</param>
        /// <returns>List&lt;Assembly&gt;.</returns>
        public static List<Assembly> GetAssemblyDependencyChain(this ICollection<Assembly> assemblies, bool descending = false)
        {
            List<AssemblyDependency> container = new List<AssemblyDependency>();

            foreach (var one in assemblies)
            {
                if (!one.GetName().IsSystemAssembly() && !one.IsDynamic)
                {
                    FillAssemblyDependency(container, one, null, 0);
                }
            }

            List<Assembly> result = new List<Assembly>();
            foreach (var one in
                (descending ?
                    (from item in container orderby item.Level descending, item.ReferencedCount descending select item)
                    : (from item in container orderby item.Level ascending, item.ReferencedCount ascending select item)))
            {
                result.Add(one.Assembly);
            }

            return result;
        }

        /// <summary>
        /// Tries the load assembly.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>System.Reflection.Assembly.</returns>
        public static Assembly TryLoadAssembly(this AssemblyName assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Fills the assembly dependency.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="lastDependency">The last dependency.</param>
        /// <param name="currentLevel">The current level.</param>
        private static void FillAssemblyDependency(List<AssemblyDependency> container, Assembly assembly, AssemblyDependency lastDependency, int currentLevel)
        {
            if (assembly == null || container == null)
            {
                return;
            }

            try
            {
                var found = (from item in container where item.Assembly == assembly select item).FirstOrDefault();
                AssemblyDependency thisDependency = null;

                if (found == null)
                {
                    thisDependency = new AssemblyDependency
                    {
                        Last = lastDependency,
                        Assembly = assembly,
                        Level = currentLevel
                    };

                    container.Add(thisDependency);
                }
                else
                {
                    thisDependency = found;

                    if (thisDependency.Level > 256)
                    {
                        return;
                    }

                    if (currentLevel > thisDependency.Level)
                    {
                        thisDependency.Level = currentLevel;
                        if (thisDependency.Last != null)
                        {
                            thisDependency.Last.Next = thisDependency.Next;
                        }

                        if (thisDependency.Next != null)
                        {
                            thisDependency.Next.Last = thisDependency.Last;
                        }

                        thisDependency.Last = lastDependency;
                        thisDependency.Next = null;
                    }
                }

                thisDependency.ReferencedCount++;

                foreach (var one in assembly.GetReferencedAssemblies())
                {
                    if (!one.IsSystemAssembly())
                    {
                        FillAssemblyDependency(container, one.TryLoadAssembly(), thisDependency, currentLevel + 1);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.Handle(new { assembly = assembly?.FullName, currentLevel });
            }
        }

        #endregion Assembly Dependency Chain

        #region GetFullName

        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <param name="methodBase">The method base.</param>
        /// <param name="requiredAttribute">The required attribute.</param>
        /// <returns>System.String.</returns>
        public static string GetFullName(this MethodBase methodBase, Type requiredAttribute = null)
        {
            return (methodBase != null && (requiredAttribute == null || methodBase.GetCustomAttribute(requiredAttribute) != null)) ?
                string.Format("{0}.{1}", methodBase.DeclaringType.GetFullName(), methodBase.Name)
                : string.Empty;
        }

        /// <summary>
        /// Gets the full name. Actually, it returns as code look.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="requiredAttribute">The required attribute.</param>
        /// <returns>System.String.</returns>
        public static string GetFullName(this Type type, Type requiredAttribute = null)
        {
            if (type != null && (requiredAttribute == null || type.GetCustomAttribute(requiredAttribute) != null))
            {
                return type.ToCodeLook();
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion GetFullName

        #region Get Current Method info

        /// <summary>
        /// Gets the current method.
        /// </summary>
        /// <returns>System.Reflection.MethodBase.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static MethodBase GetCurrentMethod()
        {
            return new StackFrame(0).GetMethod();
        }

        #endregion Get Current Method info

        /// <summary>
        /// Gets the instance fully accessiable properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static PropertyInfo[] GetInstanceFullyPublicAccessiableProperties(this Type type)
        {
            return type?.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | BindingFlags.SetProperty)?.Where(IsFullyPublic)?.ToArray();
        }

        /// <summary>
        /// Gets the instance fully accessiable fields.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static FieldInfo[] GetInstanceFullyPublicAccessiableFields(this Type type)
        {
            return type?.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.Where(IsFullyPublic)?.ToArray();
        }

        /// <summary>
        /// Determines whether [is fully public].
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>
        ///   <c>true</c> if [is fully public] [the specified property information]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsFullyPublic(this PropertyInfo propertyInfo)
        {
            return (propertyInfo?.GetGetMethod()?.IsPublic ?? false) && (propertyInfo?.GetSetMethod()?.IsPublic ?? false);
        }

        /// <summary>
        /// Determines whether [is fully public].
        /// </summary>
        /// <param name="fieldInfo">The field information.</param>
        /// <returns>
        ///   <c>true</c> if [is fully public] [the specified field information]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsFullyPublic(this FieldInfo fieldInfo)
        {
            return fieldInfo?.IsPublic ?? false;
        }

        /// <summary>
        /// Gets the custom attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembly">The assembly.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(this Assembly assembly, bool inherit = true)
            where T : Attribute
        {
            return assembly?.GetCustomAttributes(typeof(T), inherit).SafeFirstOrDefault() as T;
        }
    }
}