// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Formatters;

using Aqua.GraphCompare.Formatters.StringTransformers;
using System.Collections.Generic;
using System.Reflection;

public class BreadcrumbFormatter : IBreadcrumbFormatter
{
    public IBreadcrumbItemFormatProvider? ItemFormatProvider { get; init; }

    public IList<IStringTransformer> DisplayValueTransformers { get; init; } = new List<IStringTransformer>
        {
            new GetStringOrEmptyIfNull(),
            new CamelCaseSplitter(),
            new FirstLetterUpperCase(),
            new Trim(),
        };

    public virtual string? GetPropertyDisplayValue(Breadcrumb? breadcrumb)
    {
        if (breadcrumb is null)
        {
            return null;
        }

        var property = breadcrumb.PropertyTo ?? breadcrumb.PropertyFrom;
        if (property is not null)
        {
            var displayStringAttribute = property.GetCustomAttribute<DisplayStringAttribute>();
            if (displayStringAttribute is not null)
            {
                return displayStringAttribute.DisplayString;
            }
        }

        return GetDisplayString(breadcrumb);
    }

    public virtual string? FormatBreadcrumb(Breadcrumb? breadcrumb)
    {
        if (breadcrumb is null)
        {
            return null;
        }

        var s1 = FormatBreadcrumb(breadcrumb.Parent);

        var s2 = (breadcrumb.PropertyTo ?? breadcrumb.PropertyFrom) is null
            ? GetDisplayString(breadcrumb)
            : GetPropertyDisplayValue(breadcrumb);

        var format = string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)
            ? "{0}{1}"
            : ItemFormatProvider?.TryGetFormat(breadcrumb.Parent!, out var itemFormat) is true && itemFormat is string
            ? itemFormat
            : "{0} > {1}";

        return string.Format(format, s1, s2);
    }

    private string? GetDisplayString(Breadcrumb breadcrumb)
    {
        var displayString = breadcrumb.DisplayString;

        var property = breadcrumb.PropertyTo ?? breadcrumb.PropertyFrom;

        if (displayString is null && property is not null)
        {
            displayString = FormatString(property.Name);
        }

        return displayString;
    }

    private string? FormatString(string? value)
    {
        foreach (var transformer in DisplayValueTransformers)
        {
            value = transformer.Transform(value);
        }

        return value;
    }
}