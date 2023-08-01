// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ComparisonResult
    {
        public ComparisonResult(Type? fromType, Type? toType, IReadOnlyCollection<Delta> deltas)
        {
            if (fromType is null && toType is null)
            {
                throw new ArgumentException($"Only one of '{nameof(fromType)}' and '{nameof(toType)}' may be null.");
            }

            FromType = fromType;
            ToType = toType;
            Deltas = deltas.CheckNotNull(nameof(deltas));
        }

        public Type? FromType { get; }

        public Type? ToType { get; }

        public IReadOnlyCollection<Delta> Deltas { get; }

        public bool IsMatch => !Deltas.Any();

        public SimpleComparisonResult AsSimpleResult() => new SimpleComparisonResult(this);
    }
}
