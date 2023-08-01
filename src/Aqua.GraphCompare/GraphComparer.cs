// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare;

using Aqua.Dynamic;
using Aqua.TypeExtensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

public class GraphComparer : GraphComparerBase
{
    private readonly Func<object?, PropertyInfo?, string?>? _instanceDisplayStringProvider;
    private readonly Func<object?, PropertyInfo?, string?>? _propertyValueDisplayStringProvider;
    private readonly Func<object?, DynamicObjectWithOriginalReference?>? _objectMapper;
    private readonly Func<PropertyInfo, bool>? _propertyFilter;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphComparer"/> class
    /// with custom logic injected via function delegates.
    /// </summary>
    /// <param name="instanceDisplayStringProvider">Optional function delegate to create display strings for breadcrumb levels.</param>
    /// <param name="propertyValueDisplayStringProvider">Optional function delegate to create display strings for property values.</param>
    /// <param name="objectMapper">Optional function to map object instances to dynamoc objects for comparison.</param>
    /// <param name="propertyFilter">Optional function to define properties to be compared for a given type.</param>
    public GraphComparer(
        Func<object?, PropertyInfo?, string?>? instanceDisplayStringProvider = null,
        Func<object?, PropertyInfo?, string?>? propertyValueDisplayStringProvider = null,
        Func<object?, DynamicObjectWithOriginalReference?>? objectMapper = null,
        Func<PropertyInfo, bool>? propertyFilter = null)
    {
        _instanceDisplayStringProvider = instanceDisplayStringProvider;
        _propertyValueDisplayStringProvider = propertyValueDisplayStringProvider;
        _objectMapper = objectMapper;
        _propertyFilter = propertyFilter;
    }

    protected override DynamicObjectWithOriginalReference? MapObject(object? obj)
        => _objectMapper is null ? base.MapObject(obj) : _objectMapper(obj);

    protected override string? GetInstanceDisplayString(object? fromObj, object? toObj, PropertyInfo? fromProperty, PropertyInfo? toProperty)
    {
        fromObj = TryUnwrapDynamicObject(fromObj);
        toObj = TryUnwrapDynamicObject(toObj);

        var obj = SelectObjectForDisplayString(fromObj, toObj);
        if (obj is null)
        {
            return null;
        }

        var property = SelectPropertyForDisplayString(fromProperty, toProperty);
        var objType = obj.GetType();

        var isSingleValueProperty = property is not null && !typeof(System.Collections.Generic.IEnumerable<>).MakeGenericType(objType).IsAssignableFrom(property.PropertyType);

        if (isSingleValueProperty)
        {
            var propertyDisplayStringAttribute = property.GetCustomAttribute<DisplayStringAttribute>();
            if (propertyDisplayStringAttribute is not null)
            {
                return propertyDisplayStringAttribute.DisplayString;
            }
        }

        var displayStringAttribute = objType.GetTypeInfo().GetCustomAttribute<DisplayStringAttribute>();
        if (displayStringAttribute is not null)
        {
            return displayStringAttribute.DisplayString;
        }

        if (_instanceDisplayStringProvider is not null)
        {
            return _instanceDisplayStringProvider(obj, property);
        }

        if (isSingleValueProperty)
        {
            return property!.Name;
        }

        return obj.ToString();
    }

    protected override string? GetPropertyValueDisplayString(PropertyInfo? property, object? obj)
    {
        if (obj is null)
        {
            return null;
        }

        obj = TryUnwrapDynamicObject(obj);

        var member = TryGetEnumMember(property, obj);
        if (member is not null)
        {
            var displayStringAttribute = member.GetCustomAttribute<DisplayStringAttribute>();
            if (displayStringAttribute is not null)
            {
                return displayStringAttribute.DisplayString;
            }
        }

        if (_propertyValueDisplayStringProvider is not null)
        {
            return _propertyValueDisplayStringProvider(obj, property);
        }

        return null;
    }

    protected virtual object? SelectObjectForDisplayString(object? fromObj, object? toObj)
        => toObj ?? fromObj;

    protected virtual PropertyInfo? SelectPropertyForDisplayString(PropertyInfo? fromProperty, PropertyInfo? toProperty)
        => toProperty ?? fromProperty;

    protected override bool IsComparableProperty(PropertyInfo property)
        => base.IsComparableProperty(property) && (_propertyFilter is null || _propertyFilter(property));

    [return: NotNullIfNotNull(nameof(obj))]
    private static object? TryUnwrapDynamicObject(object? obj)
        => obj is DynamicObjectWithOriginalReference dynamicObject && dynamicObject.OriginalObject is not null
        ? dynamicObject.OriginalObject
        : obj;

    private static FieldInfo? TryGetEnumMember(PropertyInfo? property, object obj)
    {
        Type? enumType;
        if (property is null)
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
            if (value is not null)
            {
                return enumType.GetField(value.ToString());
            }
        }

        return null;
    }

    private static bool TryGetEnumType(Type type, [NotNullWhen(true)] out Type? enumType)
    {
        enumType = null;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
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

        return enumType is not null;
    }
}