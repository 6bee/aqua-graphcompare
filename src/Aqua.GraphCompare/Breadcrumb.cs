// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using Aqua.Dynamic;
    using System;
    using System.Reflection;

    public class Breadcrumb
    {
        public sealed class Item
        {
            private readonly DynamicObject _dynamicObject;

            private readonly object _instance;

            public Item(DynamicObject dynamicObject, object instance)
            {
                _dynamicObject = dynamicObject;
                _instance = instance;
            }

            public DynamicObject DynamicObject => _dynamicObject;

            public object Instance => _instance;

            public Type InstanceType => ReferenceEquals(null, _instance) ? null : _instance.GetType();
        }

        private readonly Lazy<string> _displayString;

        public Breadcrumb(Breadcrumb parent, PropertyInfo propertyFrom, PropertyInfo propertyTo, DynamicObject fromObject, object fromInstance, DynamicObject toObject, object toInstance, Func<string> displayString)
        {
            _displayString = new Lazy<string>(displayString);
            Parent = parent;
            PropertyFrom = propertyFrom;
            PropertyTo = propertyTo;
            ItemFrom = ReferenceEquals(null, fromObject) && ReferenceEquals(null, fromInstance) ? null : new Item(fromObject, fromInstance);
            ItemTo = ReferenceEquals(null, toObject) && ReferenceEquals(null, toInstance) ? null : new Item(toObject, toInstance);
        }

        internal Breadcrumb(DynamicObjectWithOriginalReference fromValue, DynamicObjectWithOriginalReference toValue, Func<string> displayString)
            : this(null, null, null, fromValue, ReferenceEquals(null, fromValue) ? null : fromValue.OriginalObject, toValue, ReferenceEquals(null, toValue) ? null : toValue.OriginalObject, displayString)
        {
        }

        internal Breadcrumb AddLevel(DynamicObjectWithOriginalReference fromValue, DynamicObjectWithOriginalReference toValue, Func<string> displayString, PropertyInfo propertyFrom, PropertyInfo propertyTo)
        {
            return new Breadcrumb(this, propertyFrom, propertyTo, fromValue, ReferenceEquals(null, fromValue) ? null : fromValue.OriginalObject, toValue, ReferenceEquals(null, toValue) ? null : toValue.OriginalObject, displayString);
        }

        internal Breadcrumb AddLevel(object fromInstance, object toInstance, Func<string> displayString, PropertyInfo propertyFrom, PropertyInfo propertyTo)
        {
            return new Breadcrumb(this, propertyFrom, propertyTo, null, fromInstance, null, toInstance, displayString);
        }

        public string DisplayString => _displayString.Value;

        public string Path => ReferenceEquals(null, Parent) ? null : Parent.ToString();

        public Breadcrumb Parent { get; }

        public PropertyInfo PropertyFrom { get; }

        public PropertyInfo PropertyTo { get; }

        public Item ItemFrom { get; }

        public Item ItemTo { get; }

        public override string ToString()
        {
            if (ReferenceEquals(null, Parent))
            {
                return DisplayString;
            }

            var displayString = DisplayString;

            var path = Path;

            var separator = string.IsNullOrEmpty(path) || string.IsNullOrEmpty(displayString) ? null : " > ";

            return string.Format("{0}{2}{1}", path, displayString, separator);
        }
    }
}
