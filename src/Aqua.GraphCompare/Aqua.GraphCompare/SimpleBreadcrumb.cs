// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using System;
    using System.Reflection;

    public sealed class SimpleBreadcrumb
    {
        private readonly Breadcrumb _breadcrumb;

        private readonly Lazy<SimpleBreadcrumb> _parent;

        public SimpleBreadcrumb(Breadcrumb breadcrumb)
        {
            _breadcrumb = breadcrumb;
            _parent = new Lazy<SimpleBreadcrumb>(() => ReferenceEquals(null, _breadcrumb.Parent) ? null : new SimpleBreadcrumb(_breadcrumb.Parent));
        }

        public string Path
        {
            get
            {
                return _breadcrumb.Path;
            }
        }

        public SimpleBreadcrumb Parent
        {
            get
            {
                return _parent.Value;
            }
        }

        public PropertyInfo Property
        {
            get
            {
                return _breadcrumb.PropertyTo ?? _breadcrumb.PropertyFrom;
            }
        }

        public Breadcrumb.Item ItemFrom
        {
            get
            {
                return _breadcrumb.ItemFrom;
            }
        }

        public Breadcrumb.Item ItemTo
        {
            get
            {
                return _breadcrumb.ItemTo;
            }
        }

        public override string ToString()
        {
            return _breadcrumb.ToString();
        }
    }
}
