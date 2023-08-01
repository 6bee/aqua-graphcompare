﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using Aqua.Dynamic;

    public sealed class DynamicObjectWithOriginalReference : DynamicObject
    {
        public DynamicObjectWithOriginalReference(DynamicObject dynamicObject, object originalObject)
            : base(dynamicObject?.Type, dynamicObject?.Properties)
        {
            dynamicObject.AssertNotNull(nameof(dynamicObject));
            OriginalObject = originalObject.CheckNotNull(nameof(originalObject));
        }

        public DynamicObjectWithOriginalReference(object obj, IDynamicObjectMapper? mapper = null)
            : base(obj, null, mapper)
            => OriginalObject = obj.CheckNotNull(nameof(obj));

        /// <summary>
        /// Gets the object instance represented by this dynamic object.
        /// </summary>
        public object OriginalObject { get; }
    }
}