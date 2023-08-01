// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Formatters.StringTransformers;

public class CamelCaseSplitter : RegexReplaceByPattern
{
    public CamelCaseSplitter()
        : base(@"(\p{Ll}(?=\p{Lu})|\p{Lu}(?=\p{Lu}\p{Ll})|[\p{Lu}\p{Ll}](?=[0-9]))", "$1 ")
    {
    }
}