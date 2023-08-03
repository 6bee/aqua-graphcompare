// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare;

using Aqua.Dynamic;
using Aqua.TypeSystem;
using System;
using System.Diagnostics.CodeAnalysis;

public class DynamicObjectMapperWithOriginalReference : DynamicObjectMapper
{
    public DynamicObjectMapperWithOriginalReference(DynamicObjectMapperSettings? settings = null, ITypeResolver? typeResolver = null, ITypeMapper? typeMapper = null, IDynamicObjectFactory? dynamicObjectFactory = null, IIsKnownTypeProvider? isKnownTypeProvider = null, ITypeSafetyChecker? typeSafetyChecker = null)
        : base(settings, typeResolver, typeMapper, dynamicObjectFactory, isKnownTypeProvider, typeSafetyChecker)
    {
    }

    [return: NotNullIfNotNull("obj")]
    public new DynamicObjectWithOriginalReference? MapObject(object? obj, Func<Type, bool>? setTypeInformation = null)
       => (DynamicObjectWithOriginalReference?)base.MapObject(obj, setTypeInformation);

    protected override DynamicObject? MapToDynamicObjectGraph(object? obj, Func<Type, bool> setTypeInformation)
    {
        if (obj is null)
        {
            return null;
        }

        var dynamicObject = (obj as DynamicObject) ?? base.MapToDynamicObjectGraph(obj, setTypeInformation);

        if (dynamicObject is not null && dynamicObject is not DynamicObjectWithOriginalReference)
        {
            dynamicObject = new DynamicObjectWithOriginalReference(dynamicObject, obj);
        }

        return dynamicObject;
    }
}