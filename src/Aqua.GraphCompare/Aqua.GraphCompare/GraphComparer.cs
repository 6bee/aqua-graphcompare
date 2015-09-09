// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using System;
    using System.Reflection;

    public class GraphComparer : GraphComparerBase
    {
        private readonly Func<object, PropertyInfo, string> _displayStringProvider;

        private readonly Func<object, PropertyInfo, string> _propertyDisplayStringProvider;

        private readonly Func<object, DynamicObjectWithOriginalReference> _objectMapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayStringProvider">Optional function delegate to create display strings for breadcrumb levels</param>
        /// <param name="propertyDisplayStringProvider">Optional function delegate to create display strings for property values.</param>
        /// <param name="objectMapper">Optional function to map object instances to dynamoc objects for comparison.</param>
        public GraphComparer(Func<object, PropertyInfo, string> displayStringProvider = null, Func<object, PropertyInfo, string> propertyDisplayStringProvider = null, Func<object,DynamicObjectWithOriginalReference> objectMapper = null)
        {
            _displayStringProvider = displayStringProvider;
            _propertyDisplayStringProvider = propertyDisplayStringProvider;
            _objectMapper = objectMapper;
        }

        protected override DynamicObjectWithOriginalReference MapObject(object obj)
        {
            return ReferenceEquals(null, _objectMapper) ? base.MapObject(obj) : _objectMapper(obj);
        }

        protected override string GetDisplayString(DynamicObjectWithOriginalReference fromObj, DynamicObjectWithOriginalReference toObj, PropertyInfo fromProperty, PropertyInfo toProperty)
        {
            var obj = SelectObjectForDisplayString(fromObj, toObj);

            var property = SelectPropertyForDisplayString(fromProperty, toProperty);

            if (ReferenceEquals(null, obj))
            {
                return null;
            }

            var displayStringAttribute = obj.Type.Type.GetCustomAttribute<DisplayStringAttribute>();
            if (displayStringAttribute != null)
            {
                return displayStringAttribute.DisplayString;
            }

            if (!ReferenceEquals(null, _displayStringProvider))
            {
                return _displayStringProvider(obj.OriginalObject, property);
            }

            if (!ReferenceEquals(null, property))
            {
                if (ReferenceEquals(null, obj.OriginalObject) || !typeof(System.Collections.Generic.IEnumerable<>).MakeGenericType(obj.OriginalObject.GetType()).IsAssignableFrom(property.PropertyType))
                {
                    return property.Name;
                }
            }

            if (!ReferenceEquals(null, obj.OriginalObject))
            {
                return obj.OriginalObject.ToString();
            }

            return null;
        }

        protected virtual DynamicObjectWithOriginalReference SelectObjectForDisplayString(DynamicObjectWithOriginalReference fromObj, DynamicObjectWithOriginalReference toObj)
        {
            return toObj ?? fromObj;
        }

        protected virtual PropertyInfo SelectPropertyForDisplayString(PropertyInfo fromProperty, PropertyInfo toProperty)
        {
            return toProperty ?? fromProperty;
        }

        protected override string GetPropertyDisplayValue(PropertyInfo property, DynamicObjectWithOriginalReference obj)
        {
            if (!ReferenceEquals(null, _propertyDisplayStringProvider))
            {
                return _propertyDisplayStringProvider(obj.OriginalObject, property);
            }

            return null;
        }
    }
}
