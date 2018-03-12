// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using Aqua.TypeSystem.Extensions;
    using System;
    using System.Reflection;

    public class GraphComparer : GraphComparerBase
    {
        private readonly Func<object, PropertyInfo, string> _instanceDisplayStringProvider;

        private readonly Func<object, PropertyInfo, string> _propertyValueDisplayStringProvider;

        private readonly Func<object, DynamicObjectWithOriginalReference> _objectMapper;

        private readonly Func<PropertyInfo, bool> _propertyFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphComparer"/> class
        /// with custom logic injected via function delegates
        /// </summary>
        /// <param name="instanceDisplayStringProvider">Optional function delegate to create display strings for breadcrumb levels</param>
        /// <param name="propertyValueDisplayStringProvider">Optional function delegate to create display strings for property values.</param>
        /// <param name="objectMapper">Optional function to map object instances to dynamoc objects for comparison.</param>
        /// <param name="propertyFilter">Optional function to define properties to be compared for a given type.</param>
        public GraphComparer(
            Func<object, PropertyInfo, string> instanceDisplayStringProvider = null,
            Func<object, PropertyInfo, string> propertyValueDisplayStringProvider = null,
            Func<object, DynamicObjectWithOriginalReference> objectMapper = null,
            Func<PropertyInfo, bool> propertyFilter = null)
        {
            _instanceDisplayStringProvider = instanceDisplayStringProvider;
            _propertyValueDisplayStringProvider = propertyValueDisplayStringProvider;
            _objectMapper = objectMapper;
            _propertyFilter = propertyFilter;
        }

        protected override DynamicObjectWithOriginalReference MapObject(object obj)
            => ReferenceEquals(null, _objectMapper) ? base.MapObject(obj) : _objectMapper(obj);

        protected override string GetInstanceDisplayString(object fromObj, object toObj, PropertyInfo fromProperty, PropertyInfo toProperty)
        {
            fromObj = TryUnwrapDynamicObject(fromObj);
            toObj = TryUnwrapDynamicObject(toObj);

            var obj = SelectObjectForDisplayString(fromObj, toObj);

            var property = SelectPropertyForDisplayString(fromProperty, toProperty);

            if (ReferenceEquals(null, obj))
            {
                return null;
            }

            var objType = obj.GetType();

            var isSingleValueProperty =
                !ReferenceEquals(null, property) &&
                !typeof(System.Collections.Generic.IEnumerable<>).MakeGenericType(objType).IsAssignableFrom(property.PropertyType);

            if (isSingleValueProperty)
            {
                var propertyDisplayStringAttribute = property.GetCustomAttribute<DisplayStringAttribute>();
                if (!ReferenceEquals(null, propertyDisplayStringAttribute))
                {
                    return propertyDisplayStringAttribute.DisplayString;
                }
            }

            var displayStringAttribute = objType.GetTypeInfo().GetCustomAttribute<DisplayStringAttribute>();
            if (!ReferenceEquals(null, displayStringAttribute))
            {
                return displayStringAttribute.DisplayString;
            }

            if (!ReferenceEquals(null, _instanceDisplayStringProvider))
            {
                return _instanceDisplayStringProvider(obj, property);
            }

            if (isSingleValueProperty)
            {
                return property.Name;
            }

            return obj.ToString();
        }

        protected override string GetPropertyValueDisplayString(PropertyInfo property, object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return null;
            }

            obj = TryUnwrapDynamicObject(obj);

            var member = TryGetEnumMember(property, obj);
            if (!ReferenceEquals(null, member))
            {
                var displayStringAttribute = member.GetCustomAttribute<DisplayStringAttribute>();
                if (!ReferenceEquals(null, displayStringAttribute))
                {
                    return displayStringAttribute.DisplayString;
                }
            }

            if (!ReferenceEquals(null, _propertyValueDisplayStringProvider))
            {
                return _propertyValueDisplayStringProvider(obj, property);
            }

            return null;
        }

        protected virtual object SelectObjectForDisplayString(object fromObj, object toObj)
            => toObj ?? fromObj;

        protected virtual PropertyInfo SelectPropertyForDisplayString(PropertyInfo fromProperty, PropertyInfo toProperty)
            => toProperty ?? fromProperty;

        protected override bool IsComparableProperty(PropertyInfo property)
            => base.IsComparableProperty(property) && (ReferenceEquals(null, _propertyFilter) || _propertyFilter(property));

        private static object TryUnwrapDynamicObject(object obj)
        {
            var dynamicObject = obj as DynamicObjectWithOriginalReference;
            if (!ReferenceEquals(null, dynamicObject) && !ReferenceEquals(null, dynamicObject.OriginalObject))
            {
                return dynamicObject.OriginalObject;
            }

            return obj;
        }

        private static FieldInfo TryGetEnumMember(PropertyInfo property, object obj)
        {
            Type enumType;
            if (ReferenceEquals(null, property))
            {
                var objType = obj.GetType();
                if (TryGetEnumType(objType, out enumType))
                {
                    return enumType.GetField(obj.ToString());
                }
            }
            else if (TryGetEnumType(property.PropertyType, out enumType))
            {
                var value = property.GetValue(obj);
                if (!ReferenceEquals(null, value))
                {
                    return enumType.GetField(value.ToString());
                }
            }

            return null;
        }

        private static bool TryGetEnumType(Type type, out Type enumType)
        {
            enumType = null;

            if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var genericArgument = type.GetGenericArguments()[0];
                if (genericArgument.IsEnum())
                {
                    enumType = genericArgument;
                }
            }
            else if (type.IsEnum())
            {
                enumType = type;
            }

            return !ReferenceEquals(null, enumType);
        }
    }
}
