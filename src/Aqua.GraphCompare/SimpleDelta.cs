// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare;

using System;
using System.Reflection;

public sealed class SimpleDelta
{
    private readonly Delta _delta;
    private readonly Lazy<SimpleBreadcrumb> _breadcrumb;

    internal SimpleDelta(Delta delta)
    {
        _delta = delta.CheckNotNull();
        _breadcrumb = new Lazy<SimpleBreadcrumb>(() => new SimpleBreadcrumb(_delta.Breadcrumb));
    }

    public ChangeType ChangeType => _delta.ChangeType;

    public SimpleBreadcrumb Breadcrumb => _breadcrumb.Value;

    public object? OldValue => _delta.Old.Value;

    public object? NewValue => _delta.New.Value;

    public string? OldDisplayValue => _delta.Old.DisplayValue;

    public string? NewDisplayValue => _delta.New.DisplayValue;

    public PropertyInfo? Property => Breadcrumb.Property;

    public override string? ToString()
        => _delta.ToString();
}