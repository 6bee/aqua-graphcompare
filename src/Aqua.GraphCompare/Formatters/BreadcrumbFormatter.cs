// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Formatters
{
    using Aqua.GraphCompare.Formatters.StringTransformers;
    using Aqua.TypeSystem.Extensions;
    using System.Collections.Generic;
    using System.Reflection;

    public class BreadcrumbFormatter : IBreadcrumbFormatter
    {
        public const string BreadcrumbSeparator = " > ";

        private readonly List<IStringTransformer> _stringTransformers;

        public BreadcrumbFormatter()
        {
            _stringTransformers = new List<IStringTransformer>()
            {
                new GetStringOrEmptyIfNull(),
                new CamelCaseSplitter(),
                new FirstLetterUpperCase(),
                new Trim(),
            };
        }

        public IList<IStringTransformer> DisplayValueTransformers => _stringTransformers;

        public virtual string GetPropertyDisplayValue(Breadcrumb breadcrumb)
        {
            var property = breadcrumb.PropertyTo ?? breadcrumb.PropertyFrom;

            if (!ReferenceEquals(null, property))
            {
                var displayStringAttribute = property.GetCustomAttribute<DisplayStringAttribute>();
                if (displayStringAttribute != null)
                {
                    return displayStringAttribute.DisplayString;
                }
            }

            return GetDisplayString(breadcrumb);
        }

        public virtual string FormatBreadcrumb(Breadcrumb breadcrumb)
        {
            if (ReferenceEquals(null, breadcrumb.PropertyTo ?? breadcrumb.PropertyFrom))
            {
                return GetDisplayString(breadcrumb);
            }

            var s1 = FormatBreadcrumb(breadcrumb.Parent);

            var s2 = GetPropertyDisplayValue(breadcrumb);

            var separator = string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)
                ? null
                : BreadcrumbSeparator;

            return string.Format("{1}{0}{2}", separator, s1, s2);
        }

        private string GetDisplayString(Breadcrumb breadcrumb)
        {
            var displayString = breadcrumb.DisplayString;

            var property = breadcrumb.PropertyTo ?? breadcrumb.PropertyFrom;

            if (ReferenceEquals(null, displayString) && !ReferenceEquals(null, property))
            {
                displayString = FormatString(property.Name);
            }

            return displayString;
        }

        private string FormatString(string value)
        {
            foreach (var transformer in DisplayValueTransformers)
            {
                value = transformer.Transform(value);
            }

            return value;
        }
    }
}
