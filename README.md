# aqua-graphcompare

| branch | package | AppVeyor | Travis CI |
| --- | --- | --- | --- |
| `master` | [![NuGet Badge](https://buildstats.info/nuget/aqua-graphcompare?includePreReleases=true)](http://www.nuget.org/packages/aqua-graphcompare) [![MyGet Pre Release](http://img.shields.io/myget/aqua/vpre/aqua-graphcompare.svg?style=flat-square&label=myget)](https://www.myget.org/feed/aqua/package/nuget/aqua-graphcompare) | [![Build status](https://ci.appveyor.com/api/projects/status/se738mykuhel4b3q/branch/master?svg=true)](https://ci.appveyor.com/project/6bee/aqua-graphcompare/branch/master) | [![Travis build Status](https://travis-ci.org/6bee/aqua-graphcompare.svg?branch=master)](https://travis-ci.org/6bee/aqua-graphcompare?branch=master) |

### Description
Differ for arbitrary object graphs allows to compare property values starting at a pair of root objects, recording any differences while visiting all objects of the graph. 

The comparison result contains a list of deltas describing each difference found. 

The comparer may be customized by both, subtyping and dependency injection for various purposes:
* Override selection of properties for comparison for any given object type
* Specify display string provider for object instance/value labeling (breadcrumb)
* Specify display string provider for property values (old/new value display string)
* Specify custom object mapper for advanced scenario

The comparer allows comparison of independent object types and relies on object structure and values instead.


### Features
* Differ for arbitrary object graphs
* Provides hierarchical and flat deltas
* Allows for custom descriptions for types and members
* Allows for custom resolution of values (i.e. display values for enums, foreign keys, etc.)

## Sample

Compare two versions of a business object
```C#
var original = GetOriginalBusinessObject();
var changed = GetModifiedBusinessObject();


var result = new GraphComparer().Compare(original, changed);


Console.WriteLine("{0} {1} {2}", 
    result.FromType, 
    result.IsMatch ? "==" : "<>", 
    result.ToType);

foreach(var delta in result.Deltas)
{
    Console.WriteLine(delta.ChangeType);
    Console.WriteLine(delta.Breadcrumb);
    Console.WriteLine(delta.OldValue);
    Console.WriteLine(delta.NewValue);
}
```
