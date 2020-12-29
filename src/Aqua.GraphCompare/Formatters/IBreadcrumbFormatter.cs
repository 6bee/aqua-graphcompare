// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Formatters
{
    public interface IBreadcrumbFormatter
    {
        string FormatBreadcrumb(Breadcrumb breadcrumb);

        string GetPropertyDisplayValue(Breadcrumb breadcrumb);
    }
}
