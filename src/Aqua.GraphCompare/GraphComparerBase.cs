// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare;

using Aqua.Dynamic;
using Aqua.TypeExtensions;
using Aqua.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public abstract class GraphComparerBase : IGraphComparer
{
    private readonly ObjectMapper _mapper;

    protected GraphComparerBase()
        => _mapper = new ObjectMapper(IsComparableProperty);

    public ComparisonResult Compare(object? from, object? to)
    {
        if (from is null && to is null)
        {
            throw new ArgumentException($"Only one of '{nameof(from)}' and '{nameof(to)}' may be null.");
        }

        var item1 = MapObject(from);

        var item2 = MapObject(to);

        var deltas = new List<Delta>();

        var breadcrumb = new Breadcrumb(item1, item2, () => GetInstanceDisplayString(item1, item2, null, null));

        CompareInstances(breadcrumb, item1, item2, deltas, new HashSet<object>(ReferenceEqualityComparer<object>.Default));

        return new ComparisonResult(item1, item2, deltas);
    }

    protected virtual DynamicObjectWithOriginalReference? MapObject(object? obj)
        => _mapper.MapToDynamicObjectWithOriginalReference(obj);

    protected virtual void CompareInstances(Breadcrumb breadcrumb, DynamicObjectWithOriginalReference? item1, DynamicObjectWithOriginalReference? item2, List<Delta> deltas, HashSet<object> referenceTracker)
    {
        if (item1 is null && item2 is null)
        {
            throw new ArgumentException($"Only one of '{nameof(item1)}' and '{nameof(item2)}' may be null.");
        }

        if (!referenceTracker.Add(item1 ?? item2!))
        {
            return;
        }

        var changeType = GetChangeType(item1, item2);

        var addedProperties = GetPropertiesMissingInSecondInstance(item2, item1);
        foreach (var property in addedProperties)
        {
            var value2 = item2![property.Name];
            ComparePropertyValues(breadcrumb, item1, item2, changeType, null, property, null, value2, deltas, referenceTracker);
        }

        var removedProperties = GetPropertiesMissingInSecondInstance(item1, item2);
        foreach (var property in removedProperties)
        {
            var value1 = item1![property.Name];
            ComparePropertyValues(breadcrumb, item1, item2, changeType, property, null, value1, null, deltas, referenceTracker);
        }

        var existingProperties = GetPropertiesExistingInBothInstances(item1, item2);
        foreach (var property in existingProperties)
        {
            var value1 = item1![property.From.Name];
            var value2 = item2![property.To.Name];
            ComparePropertyValues(breadcrumb, item1, item2, changeType, property.From, property.To, value1, value2, deltas, referenceTracker);
        }
    }

    protected virtual void ComparePropertyValues(Breadcrumb breadcrumb, DynamicObjectWithOriginalReference? item1, DynamicObjectWithOriginalReference? item2, ChangeType changeType, PropertyInfo? propertyFrom, PropertyInfo? propertyTo, object? value1, object? value2, List<Delta> deltas, HashSet<object> referenceTracker)
    {
        var nextBreadcrumb = breadcrumb.AddLevel(item1, item2, () => GetInstanceDisplayString(value1, value2, propertyFrom, propertyTo), propertyFrom, propertyTo);

        if (IsArray(value1) || IsArray(value2))
        {
            CompareCollections(nextBreadcrumb, AsObjectArray(value1), AsObjectArray(value2), deltas, referenceTracker);
        }
        else if (value1 is not null && value2 is not null && value1.GetType() != value2.GetType())
        {
            ComparePropertyValues(breadcrumb, item1, item2, ChangeType.Insert, null, propertyTo, null, value2, deltas, referenceTracker);
            ComparePropertyValues(breadcrumb, item1, item2, ChangeType.Delete, propertyFrom, null, value1, null, deltas, referenceTracker);
        }
        else if (value1 is DynamicObjectWithOriginalReference || value2 is DynamicObjectWithOriginalReference)
        {
            CompareInstances(nextBreadcrumb, value1 as DynamicObjectWithOriginalReference, value2 as DynamicObjectWithOriginalReference, deltas, referenceTracker);
        }
        else
        {
            CompareValues(nextBreadcrumb, item1, item2, changeType, value1, value2, deltas);
        }
    }

    private static object[] AsObjectArray(object? obj)
    {
        if (obj is object[] objectArray)
        {
            return objectArray;
        }

        if (obj is System.Collections.IEnumerable enumerable)
        {
            return enumerable.Cast<object>().ToArray();
        }

        return Array.Empty<object>();
    }

    private static bool IsArray(object? obj)
        => obj is object[]
        || obj?.GetType().IsArray is true;

    protected virtual void CompareCollections(Breadcrumb breadcrumb, object?[] list1, object?[] list2, List<Delta> deltas, HashSet<object> referenceTracker)
    {
        var equatableList1 = list1.Select((x, i) => new { Value = x, Equatable = CreateEquatableCollectionItem(x, i, breadcrumb.PropertyFrom) }).ToList();
        var equatableList2 = list2.Select((x, i) => new { Value = x, Equatable = CreateEquatableCollectionItem(x, i, breadcrumb.PropertyTo) }).ToList();

        var leftOuterJoin =
            from item1 in equatableList1
            join item2 in equatableList2
                on item1.Equatable equals item2.Equatable into g
            from item2 in g.DefaultIfEmpty()
            select new
            {
                Item1 = item1.Value,
                Item2 = item2?.Value,
            };

        var rightOuterJoin =
            from item2 in equatableList2
            join item1 in equatableList1
                on item2.Equatable equals item1.Equatable into g
            from item1 in g.DefaultIfEmpty()
            select new
            {
                Item1 = item1?.Value,
                Item2 = item2.Value,
            };

        var fullOuterJoin = leftOuterJoin.Union(rightOuterJoin).ToList();

        foreach (var pair in fullOuterJoin)
        {
            var item1 = pair.Item1 as DynamicObjectWithOriginalReference;
            var item2 = pair.Item2 as DynamicObjectWithOriginalReference;

            if (item1 is null && item2 is null)
            {
                var nextBreadcrumb = breadcrumb;

                if (breadcrumb.PropertyFrom is not null || breadcrumb.PropertyTo is not null)
                {
                    object? fromInstance = null;
                    object? toInstance = null;

                    if (breadcrumb.ItemFrom?.Instance is not null && breadcrumb.PropertyFrom is not null)
                    {
                        fromInstance = breadcrumb.PropertyFrom.GetValue(breadcrumb.ItemFrom.Instance);
                    }

                    if (breadcrumb.ItemTo?.Instance is not null && breadcrumb.PropertyTo is not null)
                    {
                        toInstance = breadcrumb.PropertyTo.GetValue(breadcrumb.ItemTo.Instance);
                    }

                    nextBreadcrumb = breadcrumb.AddLevel(fromInstance, toInstance, () => null, null, null);
                }

                var changeType = GetChangeType(pair.Item1, pair.Item2);

                CompareValues(nextBreadcrumb, item1, item2, changeType, pair.Item1, pair.Item2, deltas);
            }
            else
            {
                var nextBreadcrumb = breadcrumb.AddLevel(item1, item2, () => GetInstanceDisplayString(item1, item2, breadcrumb.PropertyFrom, breadcrumb.PropertyTo), null, null);

                CompareInstances(nextBreadcrumb, item1, item2, deltas, referenceTracker);
            }
        }
    }

    protected virtual void CompareValues(Breadcrumb breadcrumb, DynamicObjectWithOriginalReference? item1, DynamicObjectWithOriginalReference? item2, ChangeType changeType, object? value1, object? value2, List<Delta> deltas)
    {
        if (!AreValuesEqual(value1, value2))
        {
            var delta = CreateDelta(breadcrumb, item1, item2, changeType, value1, value2);
            deltas.Add(delta);
        }
    }

    protected virtual Delta CreateDelta(Breadcrumb breadcrumb, DynamicObjectWithOriginalReference? item1, DynamicObjectWithOriginalReference? item2, ChangeType changeType, object? value1, object? value2)
    {
        var displayValue1 = item1 is null || breadcrumb.PropertyFrom is null ? GetPropertyValueDisplayString(null, value1) : GetPropertyValueDisplayString(breadcrumb.PropertyFrom, item1);
        var displayValue2 = item2 is null || breadcrumb.PropertyTo is null ? GetPropertyValueDisplayString(null, value2) : GetPropertyValueDisplayString(breadcrumb.PropertyTo, item2);

        return new Delta(changeType, breadcrumb, value1, value2, displayValue1, displayValue2);
    }

    protected virtual bool IsComparableProperty(PropertyInfo property)
        => property.GetCustomAttribute<IgnoreAttribute>() is null;

    private ChangeType GetChangeType(object? item1, object? item2)
    {
        if (item1 is null)
        {
            return ChangeType.Insert;
        }

        if (item2 is null)
        {
            return ChangeType.Delete;
        }

        return ChangeType.Update;
    }

    protected abstract string? GetInstanceDisplayString(object? fromObj, object? toObj, PropertyInfo? fromProperty, PropertyInfo? toProperty);

    protected abstract string? GetPropertyValueDisplayString(PropertyInfo? property, object? obj);

    protected virtual bool AreValuesEqual(object? value1, object? value2)
        => Equals(value1, value2);

    protected virtual object? CreateEquatableCollectionItem(object? item, int index, PropertyInfo? collectionProperty)
        => item is DynamicObjectWithOriginalReference dynamicObject
        ? new ComparableDynamicObject(dynamicObject)
        : item;

    private static IEnumerable<PropertyInfo> GetPropertiesMissingInSecondInstance(DynamicObject? item1, DynamicObject? item2)
    {
        if (item1?.Type is null)
        {
            return Enumerable.Empty<PropertyInfo>();
        }

        var propertyNames = item1.PropertyNames;

        if (item2 is not null)
        {
            propertyNames = propertyNames.Except(item2.PropertyNames);
        }

        var declaringType = item1.Type.ToType();

        return propertyNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(declaringType.GetProperty)
            .Where(p => p is not null);
    }

    private static IEnumerable<PropertyPair> GetPropertiesExistingInBothInstances(DynamicObject? item1, DynamicObject? item2)
    {
        if (item1?.Type is null || item2?.Type is null)
        {
            return Enumerable.Empty<PropertyPair>();
        }

        var propertyNames = item1.PropertyNames.Intersect(item2.PropertyNames);

        var declaringTypeFrom = item1.Type.ToType();
        var declaringTypeTo = item2.Type.ToType();

        return propertyNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => new PropertyPair(declaringTypeFrom.GetProperty(x), declaringTypeTo.GetProperty(x)))
            .Where(x => x.From is not null && x.To is not null);
    }

    private sealed class ComparableDynamicObject : IEquatable<ComparableDynamicObject>
    {
        public ComparableDynamicObject(DynamicObject obj)
            => DynamicObject = obj.CheckNotNull();

        public DynamicObject DynamicObject { get; }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is ComparableDynamicObject other)
            {
                return Equals(other);
            }

            return false;
        }

        public bool Equals(ComparableDynamicObject other)
            => Equals(DynamicObject, other.CheckNotNull().DynamicObject);

        private static bool Equals(DynamicObject item1, DynamicObject item2)
        {
            item1.AssertNotNull();
            item2.AssertNotNull();

            var keyProperties1 = GetKeyProperties(item1);
            var keyProperties2 = GetKeyProperties(item2);
            var keyProperties = keyProperties1.Union(keyProperties2).ToList();
            if (keyProperties.Any())
            {
                return keyProperties.All(x => (item1.TryGet(x, out var key1) | item2.TryGet(x, out var key2)) && Equals(key1, key2));
            }

            if (item1.PropertyCount == item2.PropertyCount && !item1.PropertyNames.Except(item2.PropertyNames).Any())
            {
                // comparing simple properties only - no deep comparison
                if (item1.Properties.All(x => x.Value is DynamicObject || x.Value is object[] || Equals(x.Value, item2.Get(x.Name))))
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            var keyProperties = GetKeyProperties(DynamicObject);
            return keyProperties.Any()
                ? GetHashCode(keyProperties)
                : GetHashCode(DynamicObject.PropertyNames);
        }

        private static int GetHashCode(IEnumerable<string> properties)
        {
            unchecked
            {
                var hashCode = 27;
                foreach (var property in properties)
                {
                    hashCode = (hashCode * 13) ^ property.GetHashCode();
                }

                return hashCode;
            }
        }

        private static IReadOnlyCollection<string> GetKeyProperties(DynamicObject o1)
            => o1.PropertyNames
            .Where(m =>
            {
                var p = o1.Type?.ToType().GetProperty(m);
                return p?.GetCustomAttribute<ObjectKeyAttribute>() is not null;
            })
            .OrderBy(x => x)
            .ToArray();
    }

    private sealed class ObjectMapper : DynamicObjectMapperWithOriginalReference
    {
        private sealed class IsKnownTypeProvider : IIsKnownTypeProvider
        {
            public bool IsKnownType(Type type) => type.IsEnum();
        }

        private readonly Func<PropertyInfo, bool> _propertyFilter;

        public ObjectMapper(Func<PropertyInfo, bool> propertyFilter)
            : base(isKnownTypeProvider: new IsKnownTypeProvider())
            => _propertyFilter = propertyFilter.CheckNotNull();

        public DynamicObjectWithOriginalReference? MapToDynamicObjectWithOriginalReference(object? obj)
            => MapObject(obj, setTypeInformation: t => true);

        protected override IEnumerable<PropertyInfo> GetPropertiesForMapping(Type type)
        {
            if (type.GetTypeInfo().GetCustomAttribute<IgnoreAttribute>() is not null)
            {
                return Enumerable.Empty<PropertyInfo>();
            }

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .Where(_propertyFilter)
                .ToList();

            return properties;
        }
    }

    private sealed class PropertyPair
    {
        public PropertyPair(PropertyInfo propertyFrom, PropertyInfo propertyTo)
        {
            From = propertyFrom.CheckNotNull();
            To = propertyTo.CheckNotNull();
        }

        public PropertyInfo From { get; }

        public PropertyInfo To { get; }
    }
}