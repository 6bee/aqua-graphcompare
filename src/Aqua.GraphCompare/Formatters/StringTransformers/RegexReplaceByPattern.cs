// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Formatters.StringTransformers;

using System.Text.RegularExpressions;

public class RegexReplaceByPattern : IStringTransformer
{
    private readonly string _pattern;
    private readonly string _replacement;

    public RegexReplaceByPattern(string pattern, string replacement)
    {
        _pattern = pattern.CheckNotNullOrEmpty(nameof(pattern));
        _replacement = replacement.CheckNotNull(nameof(replacement));
    }

    public string? Transform(string? value)
        => value is null
        ? null
        : Regex.Replace(value, _pattern, _replacement);
}