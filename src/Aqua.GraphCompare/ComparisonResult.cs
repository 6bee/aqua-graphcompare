// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ComparisonResult
    {
        public ComparisonResult(Type fromType, Type toType, IEnumerable<Delta> deltas)
        {
            FromType = fromType;
            ToType = toType;
            Deltas = deltas;
        }

        public Type FromType { get; private set; }

        public Type ToType { get; private set; }

        public IEnumerable<Delta> Deltas { get; private set; }

        public bool IsMatch
        {
            get
            {
                return !Deltas.Any();
            }
        }

        public SimpleComparisonResult AsSimpleResult()
        {
            return new SimpleComparisonResult(this);
        }
    }
}
