# aqua-graphcompare

[![GitHub license][lic-badge]][lic-link]
[![Github Workflow][pub-badge]][pub-link]

| branch | AppVeyor                | Travis CI                      |
| ---    | ---                     | ---                            |
| `main` | [![Build status][5]][6] | [![Travis build Status][7]][8] |

| package             | nuget                  | myget                        |
| ---                 | ---                    |                              |
| `aqua-graphcompare` | [![NuGet Badge][1]][2] | [![MyGet Pre Release][3]][4] |

## Description

Differ for arbitrary object graphs allows to compare property values starting at a pair of root objects, recording any differences while visiting all nodes of the object graph.

The comparison result contains a list of deltas describing each difference found.

The comparer may be customized by both, subtyping and dependency injection for various purposes:

* Override selection of properties for comparison for any given object type
* Specify display string provider for object instance/value labeling (breadcrumb)
* Specify display string provider for property values (old/new value display string)
* Specify custom object mapper for advanced scenario

The comparer allows comparison of independent object types and relies on object structure and values at runtime rather than statically defined type information.

## Features

* Differ for arbitrary object graphs
* Provides hierarchical and flat deltas
* Allows for custom descriptions for types and members
* Allows for custom resolution of values (i.e. display values for enums, foreign keys, etc.)

## Sample

Compare two versions of an object graph:

```C#
var original = GetOriginalBusinessObject();
var changed = GetModifiedBusinessObject();

var result = new GraphComparer().Compare(original, changed);

Console.WriteLine("{0} {1} {2}", 
    result.FromType, 
    result.IsMatch ? "==" : "<>", 
    result.ToType);

foreach (var delta in result.Deltas)
{
    Console.WriteLine(delta.ChangeType);
    Console.WriteLine(delta.Breadcrumb);
    Console.WriteLine(delta.OldValue);
    Console.WriteLine(delta.NewValue);
}
```

[1]: https://buildstats.info/nuget/aqua-graphcompare?includePreReleases=true
[2]: http://www.nuget.org/packages/aqua-graphcompare
[3]: http://img.shields.io/myget/aqua/vpre/aqua-graphcompare.svg?style=flat-square&label=myget
[4]: https://www.myget.org/feed/aqua/package/nuget/aqua-graphcompare
[5]: https://ci.appveyor.com/api/projects/status/se738mykuhel4b3q/branch/main?svg=true
[6]: https://ci.appveyor.com/project/6bee/aqua-graphcompare/branch/main
[7]: https://travis-ci.org/6bee/aqua-graphcompare.svg?branch=main
[8]: https://travis-ci.org/6bee/aqua-graphcompare?branch=main

[lic-badge]: https://img.shields.io/github/license/6bee/aqua-graphcompare.svg
[lic-link]: https://github.com/6bee/aqua-graphcompare/blob/main/license.txt

[pub-badge]: https://github.com/6bee/aqua-graphcompare/actions/workflows/publish.yml/badge.svg
[pub-link]: https://github.com/6bee/aqua-graphcompare/actions/workflows/publish.yml
