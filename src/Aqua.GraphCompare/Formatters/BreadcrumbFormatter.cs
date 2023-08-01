// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Formatters;

using Aqua.GraphCompare.Formatters.StringTransformers;
using System.Collections.Generic;
using System.Reflection;

public class BreadcrumbFormatter : IBreadcrumbFormatter
{
    public string BreadcrumbSeparator { get; init; } = " > ";

    public IList<IStringTransformer> DisplayValueTransformers { get; } = new List<IStringTransformer>
    {
        new GetStringOrEmptyIfNull(),
        new CamelCaseSplitter(),
        new FirstLetterUpperCase(),
        new Trim(),
    };

    public virtual string? GetPropertyDisplayValue(Breadcrumb? breadcrumb)
    {
        var property = breadcrumb?.PropertyTo ?? breadcrumb?.PropertyFrom;

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
        if ((breadcrumb?.PropertyTo ?? breadcrumb?.PropertyFrom) is null)
        {
            return GetDisplayString(breadcrumb);
        }

        var s1 = FormatBreadcrumb(breadcrumb?.Parent);

        var s2 = GetPropertyDisplayValue(breadcrumb);

        var separator = string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)
            ? null
            : BreadcrumbSeparator;

        return $"{s1}{separator}{s2}";
    }

    private string? GetDisplayString(Breadcrumb? breadcrumb)
    {
        var displayString = breadcrumb?.DisplayString;

        var property = breadcrumb?.PropertyTo ?? breadcrumb?.PropertyFrom;

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