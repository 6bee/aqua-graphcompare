// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public sealed class ComparisonResult
    {
        public ComparisonResult(TypeInfo fromType, TypeInfo toType, IEnumerable<Delta> deltas)
        {
            FromType = fromType;
            ToType = toType;
            Deltas = deltas;
        }

        public TypeInfo FromType { get; private set; }

        public TypeInfo ToType { get; private set; }

        public IEnumerable<Delta> Deltas { get; private set; }

        public bool IsMatch
        {
            get
            {
                return !Deltas.Any();
            }
        }
    }
}
