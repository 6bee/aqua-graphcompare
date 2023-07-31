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
            public Item(DynamicObject dynamicObject, object instance)
            {
                DynamicObject = dynamicObject;
                Instance = instance;
            }

            public DynamicObject DynamicObject { get; }

            public object Instance { get; }

            public Type InstanceType => Instance?.GetType();
        }

        private readonly Lazy<string> _displayString;

        public Breadcrumb(Breadcrumb parent, PropertyInfo propertyFrom, PropertyInfo propertyTo, DynamicObject fromObject, object fromInstance, DynamicObject toObject, object toInstance, Func<string> displayString)
        {
            _displayString = new Lazy<string>(displayString);
            Parent = parent;
            PropertyFrom = propertyFrom;
            PropertyTo = propertyTo;
            ItemFrom = fromObject is null && fromInstance is null ? null : new Item(fromObject, fromInstance);
            ItemTo = toObject is null && toInstance is null ? null : new Item(toObject, toInstance);
        }

        internal Breadcrumb(DynamicObjectWithOriginalReference fromValue, DynamicObjectWithOriginalReference toValue, Func<string> displayString)
            : this(null, null, null, fromValue, fromValue is null ? null : fromValue.OriginalObject, toValue, toValue is null ? null : toValue.OriginalObject, displayString)
        {
        }

        internal Breadcrumb AddLevel(DynamicObjectWithOriginalReference fromValue, DynamicObjectWithOriginalReference toValue, Func<string> displayString, PropertyInfo propertyFrom, PropertyInfo propertyTo)
            => new Breadcrumb(this, propertyFrom, propertyTo, fromValue, fromValue is null ? null : fromValue.OriginalObject, toValue, toValue is null ? null : toValue.OriginalObject, displayString);

        internal Breadcrumb AddLevel(object fromInstance, object toInstance, Func<string> displayString, PropertyInfo propertyFrom, PropertyInfo propertyTo)
            => new Breadcrumb(this, propertyFrom, propertyTo, null, fromInstance, null, toInstance, displayString);

        public string DisplayString => _displayString.Value;

        public string Path => Parent?.ToString();

        public Breadcrumb Parent { get; }

        public PropertyInfo PropertyFrom { get; }

        public PropertyInfo PropertyTo { get; }

        public Item ItemFrom { get; }

        public Item ItemTo { get; }

        public override string ToString()
        {
            if (Parent is null)
            {
                return DisplayString;
            }

            var displayString = DisplayString;
            var path = Path;
            var separator = string.IsNullOrEmpty(path) || string.IsNullOrEmpty(displayString) ? null : " > ";
            return $"{path}{separator}{displayString}";
        }
    }
}