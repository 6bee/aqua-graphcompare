// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public abstract class GraphComparerBase : IGraphComparer
    {
        private static readonly object[] EmptyList = new object[0];

        private readonly ObjectMapper _mapper;

        protected GraphComparerBase()
        {
            _mapper = new ObjectMapper(IsComparableProperty);
        }

        public ComparisonResult Compare(object from, object to)
        {
            if (ReferenceEquals(null, from) && ReferenceEquals(null, to))
            {
                throw new ArgumentException("Only one of 'from' and 'to' may be null.");
            }

            var item1 = MapObject(from);

            var item2 = MapObject(to);

            var deltas = new List<Delta>();

            var breadcrumb = new Breadcrumb(item1, item2, () => GetInstanceDisplayString(item1, item2, null, null));

            CompareInstances(breadcrumb, item1, item2, deltas, new HashSet<object>(ReferenceEqualityComparer<object>.Default));

            var fromObjType = GetTypeInfo(item1);

            var toObjType = GetTypeInfo(item2);

            return new ComparisonResult(fromObjType, toObjType, deltas);
        }

        protected virtual DynamicObjectWithOriginalReference MapObject(object obj)
        {
            return _mapper.MapToDynamicObjectWithOriginalReference(obj);
        }

        protected virtual void CompareInstances(Breadcrumb breadcrumb, DynamicObjectWithOriginalReference item1, DynamicObjectWithOriginalReference item2, List<Delta> deltas, HashSet<object> referenceTracker)
        {
            if (ReferenceEquals(null, item1) && ReferenceEquals(null, item2))
            {
                throw new ArgumentException("Only one of 'from' and 'to' may be null.");
            }

            if (!referenceTracker.Add(item1 ?? item2))
            {
                return;
            }

            var changeType = GetChangeType(item1, item2);

            var addedProperties = GetPropertiesMissingInSecondInstance(item2, item1);
            foreach (var property in addedProperties)
            {
                var value2 = item2[property.Name];
                ComparePropertyValues(breadcrumb, item1, item2, changeType, null, property, null, value2, deltas, referenceTracker);
            }

            var removedProperties = GetPropertiesMissingInSecondInstance(item1, item2);
            foreach (var property in removedProperties)
            {
                var value1 = item1[property.Name];
                ComparePropertyValues(breadcrumb, item1, item2, changeType, property, null, value1, null, deltas, referenceTracker);
            }

            var existingProperties = GetPropertiesExistingInBothInstances(item1, item2);
            foreach (var property in existingProperties)
            {
                var value1 = item1[property.From.Name];
                var value2 = item2[property.To.Name];
                ComparePropertyValues(breadcrumb, item1, item2, changeType, property.From, property.To, value1, value2, deltas, referenceTracker);
            }
        }

        protected virtual void ComparePropertyValues(Breadcrumb breadcrumb, DynamicObjectWithOriginalReference item1, DynamicObjectWithOriginalReference item2, ChangeType changeType, PropertyInfo propertyFrom, PropertyInfo propertyTo, object value1, object value2, List<Delta> deltas, HashSet<object> referenceTracker)
        {
            var value1Obj = value1 as DynamicObjectWithOriginalReference;
            var value2Obj = value2 as DynamicObjectWithOriginalReference;

            var nextBreadcrumb = breadcrumb.AddLevel(item1, item2, () => GetInstanceDisplayString(value1, value2, propertyFrom, propertyTo), propertyFrom, propertyTo);

            if (IsArray(value1) || IsArray(value2))
            {
                CompareCollections(nextBreadcrumb, AsObjectArray(value1), AsObjectArray(value2), deltas, referenceTracker);
            }
            else if (!ReferenceEquals(null, value1) && !ReferenceEquals(null, value2) && value1.GetType() != value2.GetType())
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

        private static object[] AsObjectArray(object obj)
        {
            var objectArray = obj as object[];
            if (!ReferenceEquals(null, objectArray))
            {
                return objectArray;
            }

            var enumerable = obj as System.Collections.IEnumerable;
            if (!ReferenceEquals(null, enumerable))
            {
                return enumerable.Cast<object>().ToArray();
            }

            return EmptyList;
        }

        private static bool IsArray(object obj)
        {
            return obj is object[] || (!ReferenceEquals(null, obj) && obj.GetType().IsArray);
        }

        protected virtual void CompareCollections(Breadcrumb breadcrumb, object[] list1, object[] list2, List<Delta> deltas, HashSet<object> referenceTracker)
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
                    Item2 = ReferenceEquals(null, item2) ? null : item2.Value,
                };

            var rightOuterJoin =
                from item2 in equatableList2
                join item1 in equatableList1
                    on item2.Equatable equals item1.Equatable into g
                from item1 in g.DefaultIfEmpty()
                select new
                {
                    Item1 = ReferenceEquals(null, item1) ? null : item1.Value,
                    Item2 = item2.Value,
                };

            var fullOuterJoin = leftOuterJoin.Union(rightOuterJoin).ToList();

            foreach (var pair in fullOuterJoin)
            {
                var item1 = pair.Item1 as DynamicObjectWithOriginalReference;
                var item2 = pair.Item2 as DynamicObjectWithOriginalReference;

                if (ReferenceEquals(null, item1) && ReferenceEquals(null, item2))
                {
                    var nextBreadcrumb = breadcrumb;

                    if (!ReferenceEquals(null, breadcrumb.PropertyFrom) || !ReferenceEquals(null, breadcrumb.PropertyTo))
                    {
                        object fromInstance = null;
                        object toInstance = null;

                        if (!ReferenceEquals(null, breadcrumb.ItemFrom.Instance) && !ReferenceEquals(null, breadcrumb.PropertyFrom))
                        {
                            fromInstance = breadcrumb.PropertyFrom.GetValue(breadcrumb.ItemFrom.Instance);
                        }

                        if (!ReferenceEquals(null, breadcrumb.ItemTo.Instance) && !ReferenceEquals(null, breadcrumb.PropertyTo))
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

        protected virtual void CompareValues(Breadcrumb breadcrumb, DynamicObjectWithOriginalReference item1, DynamicObjectWithOriginalReference item2, ChangeType changeType, object value1, object value2, List<Delta> deltas)
        {
            if (!AreValuesEqual(value1, value2))
            {
                var delta = CreateDelta(breadcrumb, item1, item2, changeType, value1, value2);
                deltas.Add(delta);
            }
        }

        protected virtual Delta CreateDelta(Breadcrumb breadcrumb, DynamicObjectWithOriginalReference item1, DynamicObjectWithOriginalReference item2, ChangeType changeType, object value1, object value2)
        {
            var displayValue1 = ReferenceEquals(null, item1) || ReferenceEquals(null, breadcrumb.PropertyFrom) ? GetPropertyValueDisplayString(null, value1) : GetPropertyValueDisplayString(breadcrumb.PropertyFrom, item1);
            var displayValue2 = ReferenceEquals(null, item2) || ReferenceEquals(null, breadcrumb.PropertyTo) ? GetPropertyValueDisplayString(null, value2) : GetPropertyValueDisplayString(breadcrumb.PropertyTo, item2);

            return new Delta(changeType, breadcrumb, value1, value2, displayValue1, displayValue2);
        }

        protected virtual bool IsComparableProperty(PropertyInfo property)
            => property.GetCustomAttribute<IgnoreAttribute>() == null;

        private ChangeType GetChangeType(object item1, object item2)
        {
            if (ReferenceEquals(null, item1))
            {
                return ChangeType.Insert;
            }

            if (ReferenceEquals(null, item2))
            {
                return ChangeType.Delete;
            }

            return ChangeType.Update;
        }

        protected abstract string GetInstanceDisplayString(object fromObj, object toObj, PropertyInfo fromProperty, PropertyInfo toProperty);

        protected abstract string GetPropertyValueDisplayString(PropertyInfo property, object obj);

        protected virtual bool AreValuesEqual(object value1, object value2)
        {
            return object.Equals(value1, value2);
        }

        protected virtual object CreateEquatableCollectionItem(object item, int index, PropertyInfo collectionProperty)
        {
            var dynamicObject = item as DynamicObjectWithOriginalReference;

            return ReferenceEquals(null, dynamicObject) ? item : new ComparableDynamicObject(dynamicObject);
        }

        private static Type GetTypeInfo(DynamicObject obj)
        {
            if (!ReferenceEquals(null, obj) && !ReferenceEquals(null, obj.Type))
            {

                return obj.Type.Type;
            }

            return null;
        }

        private static IEnumerable<PropertyInfo> GetPropertiesMissingInSecondInstance(DynamicObject item1, DynamicObject item2)
        {
            if (ReferenceEquals(null, item1))
            {
                return Enumerable.Empty<PropertyInfo>();
            }

            var propertyNames = item1.PropertyNames;

            if (!ReferenceEquals(null, item2))
            {
                propertyNames = propertyNames.Except(item2.PropertyNames);
            }

            var declaringType = item1.Type.Type;

            return propertyNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(declaringType.GetProperty)
                .Where(p => !ReferenceEquals(null, p));
        }

        private static IEnumerable<PropertyPair> GetPropertiesExistingInBothInstances(DynamicObject item1, DynamicObject item2)
        {
            if (ReferenceEquals(null, item1) || ReferenceEquals(null, item2))
            {
                return Enumerable.Empty<PropertyPair>();
            }

            var propertyNames = item1.PropertyNames.Intersect(item2.PropertyNames);

            var declaringTypeFrom = item1.Type.Type;
            var declaringTypeTo = item2.Type.Type;

            return propertyNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => new PropertyPair(declaringTypeFrom.GetProperty(x), declaringTypeTo.GetProperty(x)))
                .Where(x => !ReferenceEquals(null, x.From) && !ReferenceEquals(null, x.To));
        }

        private sealed class ComparableDynamicObject : IEquatable<ComparableDynamicObject>
        {
            private readonly DynamicObject _obj;

            public ComparableDynamicObject(DynamicObject obj)
            {
                _obj = obj;
            }

            public DynamicObject DynamicObject => _obj;

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                var other = obj as ComparableDynamicObject;
                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                return Equals(other);
            }

            public bool Equals(ComparableDynamicObject other)
            {
                return Equals(_obj, other._obj);
            }

            private static bool Equals(DynamicObject item1, DynamicObject item2)
            {
                object key1;
                object key2;

                var keyProperties1 = GetKeyProperties(item1);
                var keyProperties2 = GetKeyProperties(item2);
                var keyProperties = keyProperties1.Union(keyProperties2).ToList();
                if (keyProperties.Any())
                {
                    return keyProperties.All(x => (item1.TryGet(x, out key1) | item2.TryGet(x, out key2)) && Equals(key1, key2));
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
                var keyProperties = GetKeyProperties(_obj);
                if (keyProperties.Any())
                {
                    return GetHashCode(keyProperties);
                }

                return GetHashCode(_obj.PropertyNames);
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

            private static IEnumerable<string> GetKeyProperties(DynamicObject o1)
            {
                return o1.PropertyNames
                    .Where(m =>
                        {
                            var p = o1.Type.Type.GetProperty(m);
                            return p != null && p.GetCustomAttribute<ObjectKeyAttribute>() != null;
                        })
                    .OrderBy(x => x)
                    .ToList();
            }
        }

        private sealed class ObjectMapper : DynamicObjectMapper
        {
            private sealed class IsKnownTypeProvider : IIsKnownTypeProvider
            {
                public bool IsKnownType(Type type) => type.IsEnum();
            }

            private readonly Func<PropertyInfo, bool> _propertyFilter;

            public ObjectMapper(Func<PropertyInfo, bool> propertyFilter)
                : base(isKnownTypeProvider: new IsKnownTypeProvider())
            {
                _propertyFilter = propertyFilter;
            }

            public DynamicObjectWithOriginalReference MapToDynamicObjectWithOriginalReference(object obj)
            {
                return (DynamicObjectWithOriginalReference)MapObject(obj, setTypeInformation: t => true);
            }

            protected override DynamicObject MapToDynamicObjectGraph(object obj, Func<Type, bool> setTypeInformation)
            {
                var dynamicObject = obj as DynamicObject;

                if (ReferenceEquals(null, dynamicObject))
                {
                    dynamicObject = base.MapToDynamicObjectGraph(obj, setTypeInformation);
                }

                if (!ReferenceEquals(null, dynamicObject) && !(dynamicObject is DynamicObjectWithOriginalReference))
                {
                    dynamicObject = new DynamicObjectWithOriginalReference(dynamicObject, obj);
                }

                return dynamicObject;
            }

            protected override IEnumerable<PropertyInfo> GetPropertiesForMapping(Type type)
            {
                if (type.GetTypeInfo().GetCustomAttribute<IgnoreAttribute>() != null)
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
                From = propertyFrom;
                To = propertyTo;
            }

            public PropertyInfo From { get; }

            public PropertyInfo To { get; }
        }
    }
}
