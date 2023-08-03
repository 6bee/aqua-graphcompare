// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare;

using System;
using System.Reflection;

internal sealed class DisplayStringProvider : IDisplayStringProvider
{
    private readonly Func<object?, PropertyInfo?, string?> _handler;

    public DisplayStringProvider(Func<object?, PropertyInfo?, string?> handler)
        => _handler = handler.CheckNotNull();

    public bool TryGetDisplayString(object? value, PropertyInfo? property, out string? displayString)
    {
        displayString = _handler(value, property);
        return true;
    }
}
