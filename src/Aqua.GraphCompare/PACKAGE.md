### About

Differ for arbitrary object graphs allows to compare property values starting at a pair of root objects, recording any differences while visiting all nodes of the object graph. 

The comparison result contains a list of deltas describing each difference found. 

The comparer may be customized by both, subtyping and dependency injection for various purposes:

* Override selection of properties for comparison for any given object type
* Specify display string provider for object instance/value labeling (breadcrumb)
* Specify display string provider for property values (old/new value display string)
* Specify custom object mapper for advanced scenario

The comparer allows comparison of independent object types and relies on object structure and values at runtime rather than statically defined type information.

### Features

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
