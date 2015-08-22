// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Formatters.StringTransformers
{
    public class GetStringOrEmptyIfNull : IStringTransformer
    {
        public string Transform(string value)
        {
            return ReferenceEquals(null, value) 
                ? string.Empty 
                : value;
        }
    }
}
