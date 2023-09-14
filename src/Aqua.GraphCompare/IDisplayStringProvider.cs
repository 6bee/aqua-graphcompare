// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare;

using System.Reflection;

public interface IDisplayStringProvider
{
    /// <summary>
    /// Provide a display string for a value-property-pair.
    /// </summary>
    /// <returns><see langword="true"/> is the value-proeprty-pair was handles, <see langword="false"/> otherwise.</returns>
    bool TryGetDisplayString(object? value, PropertyInfo? property, out string? displayString);
}
