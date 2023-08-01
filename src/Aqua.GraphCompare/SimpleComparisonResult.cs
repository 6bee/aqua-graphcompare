// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare;

using System;
using System.Collections.Generic;
using System.Linq;

public class SimpleComparisonResult
{
    private readonly ComparisonResult _comparisonResult;
    private readonly Lazy<IReadOnlyCollection<SimpleDelta>> _deltas;

    public SimpleComparisonResult(ComparisonResult comparisonResult)
    {
        _comparisonResult = comparisonResult.CheckNotNull(nameof(comparisonResult));
        _deltas = new Lazy<IReadOnlyCollection<SimpleDelta>>(() => _comparisonResult.Deltas.Select(x => new SimpleDelta(x)).ToList());
    }

    public Type Type
        => _comparisonResult.ToType
        ?? _comparisonResult.FromType
        ?? throw new InvalidOperationException($"Only one of '{nameof(_comparisonResult.FromType)}' and '{nameof(_comparisonResult.ToType)}' may be null.");

    public IReadOnlyCollection<SimpleDelta> Deltas => _deltas.Value;

    public bool IsMatch => _comparisonResult.IsMatch;
}