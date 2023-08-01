// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Formatters.StringTransformers
{
    public interface IStringTransformer
    {
        string? Transform(string? value);
    }
}
