// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare;

public interface IGraphComparer
{
    ComparisonResult Compare(object? from, object? to);
}