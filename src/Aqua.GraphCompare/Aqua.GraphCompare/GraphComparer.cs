// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using System;
    using System.Reflection;

    public class GraphComparer : GraphComparerBase
    {
        private readonly Func<object, string> _displayStringProvider;

        private readonly Func<object, PropertyInfo, string> _propertyDisplayStringProvider;

        public GraphComparer(Func<object, string> displayStringProvider = null, Func<object, PropertyInfo, string> propertyDisplayStringProvider = null)
        {
            _displayStringProvider = displayStringProvider;
            _propertyDisplayStringProvider = propertyDisplayStringProvider;
        }

        protected override string GetDisplayString(DynamicObjectWithOriginalReference fromObj, DynamicObjectWithOriginalReference toObj)
        {
            var obj = toObj ?? fromObj;

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
                return _displayStringProvider(obj.OriginalObject);
            }

            if (!ReferenceEquals(null, obj.OriginalObject))
            {
                return obj.OriginalObject.ToString();
            }

            return null;
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
