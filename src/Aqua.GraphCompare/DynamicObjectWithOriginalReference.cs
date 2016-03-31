// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using Aqua.Dynamic;

    public sealed class DynamicObjectWithOriginalReference : DynamicObject
    {
        private readonly object _originalObject;

        public DynamicObjectWithOriginalReference(DynamicObject dynamicObject, object originalObject)
            : base(dynamicObject, deepCopy: false)
        {
            _originalObject = originalObject;
        }

        public DynamicObjectWithOriginalReference(object obj, IDynamicObjectMapper mapper = null)
            : base(obj, mapper)
        {
            _originalObject = obj;
        }

        /// <summary>
        /// Gets the object instance represented by this dynamic object
        /// </summary>
        public object OriginalObject
        {
            get
            {
                return _originalObject;
            }
        }
    }
}
