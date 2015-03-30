namespace Prototypes

open Microsoft.FSharp
open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections
open System
open System.Linq
open System.Collections
open System.Collections.Generic

// Modeling a table or updatable view of some type
// akin to `seq` in QueryBuilder, but I don't want the usage of this
// type to imply that it can be operated on at all within the application
// it is merely the remote SQL table's type.
[<NoComparison; NoEquality; Sealed>]
type Table<'T> =
    private new : unit -> Table<'T>

// I don't really know what this type is for.
// 'T is pretty clearly the current projection type
// But I don't know what 'Q is
[<NoComparison; NoEquality; Sealed>]
type Target<'T, 'Q> =
    new : Table<'T> option -> Target<'T,'Q>
    member Target : Table<'T> option

// Staring with the basics, this was called `QueryBuilder`
[<Class>]
type CommandBuilder =
    new : unit -> CommandBuilder    
    member Yield : value:'T -> Target<'T,'Q>
    member Zero : unit -> Target<'T,'Q> 
    member For : target:Target<'T,'Q> * body:('T -> Target<'Result,'Q2>) -> Target<'Result,'Q>
    [<CustomOperation("select",AllowIntoPattern=true)>] 
    member Select : target:Target<'T,'Q> * [<ProjectionParameter>] projection:('T -> 'Result) -> Target<'Result,'Q>
    [<CustomOperation("where",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member Where : source:Target<'T,'Q> * [<ProjectionParameter>] predicate:('T -> bool) -> Target<'T,'Q>    
    [<CustomOperation("insertInto" )>] 
    member Insert : source:Target<'T, 'Q> * [<ProjectionParameter>] target:(unit -> Table<'T>) -> unit    
    [<CustomOperation("delete")>] 
    member Delete : target:Target<'T,'Q> -> unit


// Add back many of the below advanced mechanisms once the above it working
(*

    /// <summary>A query operator that determines whether the selected elements contains a specified element.
    /// </summary>
    [<CustomOperation("contains")>] 
    member Contains : source:Target<'T,'Q> * key:'T -> bool

    /// <summary>A query operator that returns the number of selected elements.
    /// </summary>
    [<CustomOperation("count")>] 
    member Count : source:Target<'T,'Q> -> int

    /// <summary>A query operator that selects the last element of those selected so far.
    /// </summary>
    [<CustomOperation("last")>] 
    member Last : source:Target<'T,'Q> -> 'T

    /// <summary>A query operator that selects the last element of those selected so far, or a default value if no element is found.
    /// </summary>
    [<CustomOperation("lastOrDefault")>] 
    member LastOrDefault : source:Target<'T,'Q> -> 'T

    /// <summary>A query operator that selects the single, specific element selected so far
    /// </summary>
    [<CustomOperation("exactlyOne")>] 
    member ExactlyOne : source:Target<'T,'Q> -> 'T

    /// <summary>A query operator that selects the single, specific element of those selected so far, or a default value if that element is not found.
    /// </summary>
    [<CustomOperation("exactlyOneOrDefault")>] 
    member ExactlyOneOrDefault : source:Target<'T,'Q> -> 'T

    /// <summary>A query operator that selects the first element of those selected so far, or a default value if the sequence contains no elements.
    /// </summary>
    [<CustomOperation("headOrDefault")>] 
    member HeadOrDefault : source:Target<'T,'Q> -> 'T

    /// <summary>A query operator that selects a value for each element selected so far and returns the minimum resulting value. 
    /// </summary>
    [<CustomOperation("minBy")>] 
    member MinBy : source:Target<'T,'Q> * [<ProjectionParameter>] valueSelector:('T -> 'Value) -> 'Value when 'Value : equality and 'Value : comparison

    /// <summary>A query operator that selects a value for each element selected so far and returns the maximum resulting value. 
    /// </summary>
    [<CustomOperation("maxBy")>] 
    member MaxBy : source:Target<'T,'Q> * [<ProjectionParameter>] valueSelector:('T -> 'Value) -> 'Value when 'Value : equality and 'Value : comparison

    /// <summary>A query operator that groups the elements selected so far according to a specified key selector.
    /// </summary>
    [<CustomOperation("groupBy",AllowIntoPattern=true)>] 
    member GroupBy : source:Target<'T,'Q> * [<ProjectionParameter>] keySelector:('T -> 'Key) -> Target<System.Linq.IGrouping<'Key,'T>,'Q> when 'Key : equality 

    /// <summary>A query operator that sorts the elements selected so far in ascending order by the given sorting key.
    /// </summary>
    [<CustomOperation("sortBy",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member SortBy : source:Target<'T,'Q> * [<ProjectionParameter>] keySelector:('T -> 'Key) -> Target<'T,'Q> when 'Key : equality and 'Key : comparison

    /// <summary>A query operator that sorts the elements selected so far in descending order by the given sorting key.
    /// </summary>
    [<CustomOperation("sortByDescending",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member SortByDescending : source:Target<'T,'Q> * [<ProjectionParameter>] keySelector:('T -> 'Key) -> Target<'T,'Q> when 'Key : equality and 'Key : comparison

    /// <summary>A query operator that performs a subsequent ordering of the elements selected so far in ascending order by the given sorting key.
    /// This operator may only be used immediately after a 'sortBy', 'sortByDescending', 'thenBy' or 'thenByDescending', or their nullable variants.
    /// </summary>
    [<CustomOperation("thenBy",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member ThenBy : source:Target<'T,'Q> * [<ProjectionParameter>] keySelector:('T -> 'Key) -> Target<'T,'Q> when 'Key : equality and 'Key : comparison

    /// <summary>A query operator that performs a subsequent ordering of the elements selected so far in descending order by the given sorting key.
    /// This operator may only be used immediately after a 'sortBy', 'sortByDescending', 'thenBy' or 'thenByDescending', or their nullable variants.
    /// </summary>
    [<CustomOperation("thenByDescending",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member ThenByDescending : source:Target<'T,'Q> * [<ProjectionParameter>] keySelector:('T -> 'Key) -> Target<'T,'Q> when 'Key : equality and 'Key : comparison

    /// <summary>A query operator that selects a value for each element selected so far and groups the elements by the given key.
    /// </summary>
    [<CustomOperation("groupValBy",AllowIntoPattern=true)>] 
    member GroupValBy<'T,'Key,'Value,'Q> : source:Target<'T,'Q> * [<ProjectionParameter>] resultSelector:('T -> 'Value) * [<ProjectionParameter>] keySelector:('T -> 'Key) -> Target<System.Linq.IGrouping<'Key,'Value>,'Q> when 'Key : equality 

    /// <summary>A query operator that correlates two sets of selected values based on matching keys. 
    /// Normal usage is 'join y in elements2 on (key1 = key2)'. 
    /// </summary>
    [<CustomOperation("join",IsLikeJoin=true,JoinConditionWord="on")>] 
    member Join : outerSource:Target<'Outer,'Q> * innerSource:Target<'Inner,'Q> * outerKeySelector:('Outer -> 'Key) * innerKeySelector:('Inner -> 'Key) * resultSelector:('Outer -> 'Inner -> 'Result) -> Target<'Result,'Q>

    /// <summary>A query operator that correlates two sets of selected values based on matching keys and groups the results. 
    /// Normal usage is 'groupJoin y in elements2 on (key1 = key2) into group'. 
    /// </summary>
    [<CustomOperation("groupJoin",IsLikeGroupJoin=true,JoinConditionWord="on")>] 
    member GroupJoin : outerSource:Target<'Outer,'Q> * innerSource:Target<'Inner,'Q> * outerKeySelector:('Outer -> 'Key) * innerKeySelector:('Inner -> 'Key) * resultSelector:('Outer -> seq<'Inner> -> 'Result) -> Target<'Result,'Q>

    /// <summary>A query operator that correlates two sets of selected values based on matching keys and groups the results.
    /// If any group is empty, a group with a single default value is used instead. 
    /// Normal usage is 'leftOuterJoin y in elements2 on (key1 = key2) into group'. 
    /// </summary>
    [<CustomOperation("leftOuterJoin",IsLikeGroupJoin=true,JoinConditionWord="on")>] 
    member LeftOuterJoin : outerSource:Target<'Outer,'Q> * innerSource:Target<'Inner,'Q> * outerKeySelector:('Outer -> 'Key) * innerKeySelector:('Inner -> 'Key) * resultSelector:('Outer -> seq<'Inner> -> 'Result) -> Target<'Result,'Q>

    /// <summary>A query operator that selects a nullable value for each element selected so far and returns the sum of these values. 
    /// If any nullable does not have a value, it is ignored.
    /// </summary>
    [<CustomOperation("sumByNullable")>] 
    member inline SumByNullable : source:Target<'T,'Q> * [<ProjectionParameter>] valueSelector:('T -> Nullable< ^Value >) -> Nullable< ^Value > 
                                        when ^Value : (static member ( + ) : ^Value * ^Value -> ^Value) 
                                        and  ^Value : (static member Zero : ^Value)
                                        //and default ^Value : int

    /// <summary>A query operator that selects a nullable value for each element selected so far and returns the minimum of these values. 
    /// If any nullable does not have a value, it is ignored.
    /// </summary>
    [<CustomOperation("minByNullable")>] 
    member MinByNullable : source:Target<'T,'Q> * [<ProjectionParameter>] valueSelector:('T -> Nullable<'Value>) -> Nullable<'Value> 
                                        when 'Value : equality 
                                        and 'Value : comparison  

    /// <summary>A query operator that selects a nullable value for each element selected so far and returns the maximum of these values. 
    /// If any nullable does not have a value, it is ignored.
    /// </summary>
    [<CustomOperation("maxByNullable")>] 
    member MaxByNullable : source:Target<'T,'Q> * [<ProjectionParameter>] valueSelector:('T -> Nullable<'Value>) -> Nullable<'Value> when 'Value : equality and 'Value : comparison  

    /// <summary>A query operator that selects a nullable value for each element selected so far and returns the average of these values. 
    /// If any nullable does not have a value, it is ignored.
    /// </summary>
    [<CustomOperation("averageByNullable")>] 
    member inline AverageByNullable   : source:Target<'T,'Q> * [<ProjectionParameter>] projection:('T -> Nullable< ^Value >) -> Nullable< ^Value > 
                        when ^Value : (static member ( + ) : ^Value * ^Value -> ^Value)  
                        and  ^Value : (static member DivideByInt : ^Value * int -> ^Value)  
                        and  ^Value : (static member Zero : ^Value)  
                        //and default ^Value : float


    /// <summary>A query operator that selects a value for each element selected so far and returns the average of these values. 
    /// </summary>
    [<CustomOperation("averageBy")>] 
    member inline AverageBy   : source:Target<'T,'Q> * [<ProjectionParameter>] projection:('T -> ^Value) -> ^Value 
                                    when ^Value : (static member ( + ) : ^Value * ^Value -> ^Value) 
                                    and  ^Value : (static member DivideByInt : ^Value * int -> ^Value) 
                                    and  ^Value : (static member Zero : ^Value)
                                    //and default ^Value : float


    /// <summary>A query operator that selects distinct elements from the elements selected so far. 
    /// </summary>
    [<CustomOperation("distinct",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member Distinct: source:Target<'T,'Q> -> Target<'T,'Q> when 'T : equality

    /// <summary>A query operator that determines whether any element selected so far satisfies a condition.
    /// </summary>
    [<CustomOperation("exists")>] 
    member Exists: source:Target<'T,'Q> * [<ProjectionParameter>] predicate:('T -> bool) -> bool

    /// <summary>A query operator that selects the first element selected so far that satisfies a specified condition.
    /// </summary>
    [<CustomOperation("find")>] 
    member Find: source:Target<'T,'Q> * [<ProjectionParameter>] predicate:('T -> bool) -> 'T


    /// <summary>A query operator that determines whether all elements selected so far satisfies a condition.
    /// </summary>
    [<CustomOperation("all")>] 
    member All: source:Target<'T,'Q> * [<ProjectionParameter>] predicate:('T -> bool) -> bool

    /// <summary>A query operator that selects the first element from those selected so far.
    /// </summary>
    [<CustomOperation("head")>] 
    member Head: source:Target<'T,'Q> -> 'T

    /// <summary>A query operator that selects the element at a specified index amongst those selected so far.
    /// </summary>
    [<CustomOperation("nth")>] 
    member Nth: source:Target<'T,'Q> * index:int -> 'T

    /// <summary>A query operator that bypasses a specified number of the elements selected so far and selects the remaining elements.
    /// </summary>
    [<CustomOperation("skip",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member Skip:  source:Target<'T,'Q> * count:int -> Target<'T,'Q>

    /// <summary>A query operator that bypasses elements in a sequence as long as a specified condition is true and then selects the remaining elements.
    /// </summary>
    [<CustomOperation("skipWhile",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member SkipWhile: source:Target<'T,'Q> * [<ProjectionParameter>] predicate:('T -> bool) -> Target<'T,'Q>

    /// <summary>A query operator that selects a value for each element selected so far and returns the sum of these values. 
    /// </summary>
    [<CustomOperation("sumBy")>] 
    member inline SumBy   : source:Target<'T,'Q> * [<ProjectionParameter>] projection:('T -> ^Value) -> ^Value 
                                    when ^Value : (static member ( + ) : ^Value * ^Value -> ^Value) 
                                    and  ^Value : (static member Zero : ^Value)
                                    //and default ^Value : int

    /// <summary>A query operator that selects a specified number of contiguous elements from those selected so far.
    /// </summary>
    [<CustomOperation("take",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member Take: source:Target<'T,'Q> * count:int-> Target<'T,'Q>

    /// <summary>A query operator that selects elements from a sequence as long as a specified condition is true, and then skips the remaining elements.
    /// </summary>
    [<CustomOperation("takeWhile",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member TakeWhile: source:Target<'T,'Q> * [<ProjectionParameter>] predicate:('T -> bool) -> Target<'T,'Q>

    /// <summary>A query operator that sorts the elements selected so far in ascending order by the given nullable sorting key.
    /// </summary>
    [<CustomOperation("sortByNullable",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member SortByNullable : source:Target<'T,'Q> * [<ProjectionParameter>] keySelector:('T -> Nullable<'Key>) -> Target<'T,'Q> when 'Key : equality and 'Key : comparison

    /// <summary>A query operator that sorts the elements selected so far in descending order by the given nullable sorting key.
    /// </summary>
    [<CustomOperation("sortByNullableDescending",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member SortByNullableDescending : source:Target<'T,'Q> * [<ProjectionParameter>] keySelector:('T -> Nullable<'Key>) -> Target<'T,'Q> when 'Key : equality and 'Key : comparison

    /// <summary>A query operator that performs a subsequent ordering of the elements selected so far in ascending order by the given nullable sorting key.
    /// This operator may only be used immediately after a 'sortBy', 'sortByDescending', 'thenBy' or 'thenByDescending', or their nullable variants.
    /// </summary>
    [<CustomOperation("thenByNullable",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member ThenByNullable : source:Target<'T,'Q> * [<ProjectionParameter>] keySelector:('T -> Nullable<'Key>) -> Target<'T,'Q> when 'Key : equality and 'Key : comparison

    /// <summary>A query operator that performs a subsequent ordering of the elements selected so far in descending order by the given nullable sorting key.
    /// This operator may only be used immediately after a 'sortBy', 'sortByDescending', 'thenBy' or 'thenByDescending', or their nullable variants.
    /// </summary>
    [<CustomOperation("thenByNullableDescending",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member ThenByNullableDescending : source:Target<'T,'Q> * [<ProjectionParameter>] keySelector:('T -> Nullable<'Key>) -> Target<'T,'Q> when 'Key : equality and 'Key : comparison
*)