<Query Kind="Statements">
  <Reference Relative="bin\Debug\f_sql.dll">C:\Projects\github.com\MrAndMrsK\Unbounded\CompactModelEx\bin\Debug\f_sql.dll</Reference>
  <Reference Relative="bin\Debug\FSharp.Core.dll">C:\Projects\github.com\MrAndMrsK\Unbounded\CompactModelEx\bin\Debug\FSharp.Core.dll</Reference>
  <Namespace>Prototypes</Namespace>
</Query>

var t = Tree.root(1) //.Dump("root")
		.Expand(x => Enumerable.Range(x + 1, 3)) //.Dump("Expanding")
		.Select(x => x * x) //.Dump("Select")
		.MapDepth(3)
		.Dump("Max Depth")
;

//t.Dump();

//TreeExtensions.Expand( Tree.root(1), x => Enumerable.Range(x, 2)).Dump();