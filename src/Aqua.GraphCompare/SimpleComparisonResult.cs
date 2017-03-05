// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SimpleComparisonResult
    {
        private readonly ComparisonResult _comparisonResult;

        private readonly Lazy<IEnumerable<SimpleDelta>> _deltas;

        public SimpleComparisonResult(ComparisonResult comparisonResult)
        {
            _comparisonResult = comparisonResult;
            _deltas = new Lazy<IEnumerable<SimpleDelta>>(() => _comparisonResult.Deltas.Select(x => new SimpleDelta(x)).ToList());
        }

        public Type Type => _comparisonResult.ToType ?? _comparisonResult.FromType;

        public IEnumerable<SimpleDelta> Deltas => _deltas.Value;

        public bool IsMatch => _comparisonResult.IsMatch;
    }
}
