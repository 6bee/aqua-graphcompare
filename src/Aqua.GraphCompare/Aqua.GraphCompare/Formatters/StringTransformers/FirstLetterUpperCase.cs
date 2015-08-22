// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Formatters.StringTransformers
{
    public class FirstLetterUpperCase : RegexReplaceByMatchEvaluator
    {
        public FirstLetterUpperCase()
            : base(@"^\s*(\S)", m => m.Groups[1].Value.ToUpper())
        {
        }
    }
}
