// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Formatters;

using System.Diagnostics.CodeAnalysis;

public interface IBreadcrumbItemFormatProvider
{
    /// <summary>
    /// Get the format for breadcrumb items with '{0}' for the parent path and '{1}' for the current item value.
    /// </summary>
    bool TryGetFormat(Breadcrumb breadcrumb, [NotNullWhen(true)] out string? format);
}