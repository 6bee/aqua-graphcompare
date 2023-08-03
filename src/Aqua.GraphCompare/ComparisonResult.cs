// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare;

using System;
using System.Collections.Generic;
using System.Linq;

public class ComparisonResult
{
    public ComparisonResult(DynamicObjectWithOriginalReference? from, DynamicObjectWithOriginalReference? to, IReadOnlyCollection<Delta> deltas)
    {
        if (from is null && to is null)
        {
            throw new ArgumentException($"Only one of '{nameof(from)}' and '{nameof(to)}' may be null.");
        }

        Deltas = deltas.CheckNotNull(nameof(deltas));
        From = from;
        To = to;
    }

    public DynamicObjectWithOriginalReference? From { get; }

    public DynamicObjectWithOriginalReference? To { get; }

    public Type? FromType => From?.Type?.ToType();

    public Type? ToType => To?.Type?.ToType();

    public IReadOnlyCollection<Delta> Deltas { get; }

    public bool IsMatch => !Deltas.Any();

    public SimpleComparisonResult AsSimpleResult() => new SimpleComparisonResult(this);
}