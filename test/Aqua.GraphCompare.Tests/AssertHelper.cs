// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Tests;

using Aqua.GraphCompare;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class AssertHelper
{
    public static void DisplayValuesShouldBeNull(this Delta delta)
    {
        delta.NewDisplayValue.ShouldBeNull();
        delta.OldDisplayValue.ShouldBeNull();
    }

    public static void PropertiesShouldBe(this Delta delta, PropertyInfo nameProperty)
    {
        delta.PropertyFrom.ShouldBe(nameProperty);
        delta.PropertyTo.ShouldBe(nameProperty);
    }

    public static void PropertiesShouldBeNull(this Delta delta)
    {
        delta.PropertyFrom.ShouldBeNull();
        delta.PropertyTo.ShouldBeNull();
    }

    public static void ItemTypesShouldBe<T>(this Breadcrumb breadcrumb)
    {
        breadcrumb.ItemFrom.TypesShouldBe<T>();
        breadcrumb.ItemTo.TypesShouldBe<T>();
    }

    public static void TypesShouldBe<T>(this Breadcrumb.Item item)
    {
        item.DynamicObject.Type.ToType().ShouldBe(typeof(T));
        item.Instance.ShouldBeOfType<T>();
        item.InstanceType.ShouldBe(typeof(T));
    }

    public static void PropertiesShouldBe(this Breadcrumb breadcrumb, PropertyInfo property)
    {
        breadcrumb.PropertyFrom.ShouldBe(property);
        breadcrumb.PropertyTo.ShouldBe(property);
    }

    public static void PropertiesShouldBeNull(this Breadcrumb breadcrumb)
    {
        breadcrumb.PropertyFrom.ShouldBeNull();
        breadcrumb.PropertyTo.ShouldBeNull();
    }

    public static void PathShouldBe(this Breadcrumb breadcrumb, params object[] levels)
    {
        breadcrumb.Path.ShouldBe(BuildBreadcrumbPath(levels));
    }

    private static string BuildBreadcrumbPath(params object[] values)
    {
        return string.Join(" > ", values);
    }

    public static IEnumerable<Delta> GetDeltas(this ComparisonResult result, PropertyInfo property)
    {
        return result.Deltas.Where(x => x.PropertyFrom == property || x.PropertyTo == property);
    }

    public static IEnumerable<Delta> GetDeltas<T>(this ComparisonResult result, string propertyName)
    {
        return GetDeltas(result, typeof(T).GetProperty(propertyName));
    }

    public static Delta GetDelta(this ComparisonResult result, PropertyInfo property)
    {
        return result.Deltas.Single(x => x.PropertyFrom == property || x.PropertyTo == property);
    }

    public static Delta GetDelta<T>(this ComparisonResult result, string propertyName)
    {
        return GetDelta(result, typeof(T).GetProperty(propertyName));
    }
}