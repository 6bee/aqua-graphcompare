// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Formatters.StringTransformers
{
    using System.Text.RegularExpressions;

    public class RegexReplaceByMatchEvaluator : IStringTransformer
    {
        private readonly string _pattern;

        private readonly MatchEvaluator _evaluator;

        public RegexReplaceByMatchEvaluator(string pattern, MatchEvaluator evaluator)
        {
            _pattern = pattern;
            _evaluator = evaluator;
        }

        public string Transform(string value)
        {
            return Regex.Replace(value, _pattern, _evaluator);
        }
    }
}
