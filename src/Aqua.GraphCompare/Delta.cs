// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare;

using System.Reflection;

public sealed class Delta
{
    public sealed record Reference
    {
        public Reference(object? value, string? displayValue)
        {
            Value = value;
            DisplayValue = displayValue;
        }

        public object? Value { get; }

        public string? DisplayValue { get; }

        public override string? ToString()
            => DisplayValue
            ?? (Value is string s ? @$"""{s}""" : Value is char c ? $"'{c}'" : Value?.ToString())
            ?? "[NULL]";
    }

    public Delta(ChangeType changeType, Breadcrumb breadcrumb, object? oldValue, object? newValue, string? oldDisplayValue, string? newDisplayValue)
    {
        ChangeType = changeType;
        Breadcrumb = breadcrumb.CheckNotNull();
        Old = new Reference(oldValue, oldDisplayValue);
        New = new Reference(newValue, newDisplayValue);
    }

    public ChangeType ChangeType { get; }

    public Breadcrumb Breadcrumb { get; }

    public Reference Old { get; }

    public Reference New { get; }

    public PropertyInfo? PropertyFrom => Breadcrumb.PropertyFrom;

    public PropertyInfo? PropertyTo => Breadcrumb.PropertyTo;

    public override string ToString()
        => string.Format(
            "[{0}] {1}: {2} -> {3}",
            ChangeType.ToString().ToUpper(),
            Breadcrumb,
            Old,
            New);
}