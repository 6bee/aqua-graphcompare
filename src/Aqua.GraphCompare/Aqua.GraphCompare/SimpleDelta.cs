// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using System;
    using System.Reflection;

    public sealed class SimpleDelta
    {
        private readonly Delta _delta;

        private readonly Lazy<SimpleBreadcrumb> _breadcrumb;

        internal SimpleDelta(Delta delta)
        {
            _delta = delta;
            _breadcrumb = new Lazy<SimpleBreadcrumb>(() => new SimpleBreadcrumb(_delta.Breadcrumb));
        }

        public ChangeType ChangeType
        {
            get
            {
                return _delta.ChangeType;
            }
        }

        public SimpleBreadcrumb Breadcrumb
        {
            get
            {
                return _breadcrumb.Value;
            }
        }

        public object OldValue
        {
            get
            {
                return _delta.OldValue;
            }
        }

        public object NewValue
        {
            get
            {
                return _delta.NewValue;
            }
        }

        public string OldDisplayValue
        {
            get
            {
                return _delta.OldDisplayValue;
            }
        }

        public string NewDisplayValue
        {
            get
            {
                return _delta.NewDisplayValue;
            }
        }

        public PropertyInfo Property
        {
            get
            {
                return Breadcrumb.Property;
            }
        }

        public override string ToString()
        {
            return _delta.ToString();
        }
    }
}
