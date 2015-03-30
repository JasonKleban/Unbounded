#if INTERACTIVE
#else
module example
#endif
    
open System
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.DerivedPatterns
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.ExprShape

let BodyOf op =
    match op with
    | Patterns.Call(None, DerivedPatterns.MethodWithReflectedDefinition body, args) -> body
    | Patterns.Lambda (a, Patterns.Call(None, DerivedPatterns.MethodWithReflectedDefinition body, args)) -> body
    | Patterns.Lambda (a, Lambda (b, Patterns.Call(None, DerivedPatterns.MethodWithReflectedDefinition body, args))) -> body
    | e -> e

type Domain = interface end
type Projection<'R> = 
    abstract member source : Table<'R>
and Table<'R> = inherit Projection<'R>
and Table<'R, 'K> = 
    inherit Table<('R * 'K)>
    abstract member row : 'R // including key data (?)
    abstract member key : 'K
and Join<'L, 'R, 'K> = 
    inherit Projection<('L * 'R)>
    abstract member left : Projection<'L>
    abstract member right : Projection<'R>
    abstract member leftKey : 'L -> 'K
    abstract member rightKey : 'R -> 'K
and LeftJoin<'L, 'R, 'K> = 
    inherit Projection<('L * 'R)>
    abstract member left : Projection<'L>
    abstract member right : Projection<'R> option
    abstract member leftKey : 'L -> 'K
    abstract member rightKey : 'R -> 'K

type proj<'R> = Projection<'R>
type table<'R> = Table<'R>

type Engine<'D> when 'D :> Domain = 
    abstract member from: 'R table * ('R proj -> 'U) -> 'U
    //abstract member from: _ proj * ('D -> 'U) -> ('D -> 'U)
    abstract member join: 'L proj * 'R proj * ('L -> 'K) * ('R -> 'K) -> Join<'L, 'R, 'K>
    abstract member leftjoin: 'L proj * 'R proj * ('L -> 'K) * ('R -> 'K) -> Join<'L, 'R, 'K>
    abstract member where: ('R -> bool) * 'R proj -> 'R proj
    abstract member select: ('R -> 'S) * 'R proj -> 'S proj
    abstract member update: ('R -> unit) * 'R proj -> unit
    abstract member delete: 'R table -> unit
    abstract member deleteFrom: ('R proj -> 'R table) -> unit
//    abstract member deleteFrom: (Join<'L, 'R, 'K> -> 'R table) -> unit
//    abstract member deleteFrom: (Join<'L, 'R, 'K> -> 'L table) -> unit
    abstract member lift: 'R -> 'R proj
    abstract member insert: 'R table * 'R proj -> 'R proj
    abstract member insert: Table<'K, 'F> * 'R proj -> 'K proj

type A = { ID : int; Name : string }
type B = { ID2 : int; Name2 : string }
type MyDomain =
    inherit Domain
    abstract member As : A table
    abstract member Bs : B table

type CommandBuilder<'D> when 'D :> Domain () =
    member __.For ([<ProjectionParameter>] over : ('D -> 'R table), expr : ('R proj -> _)) = 
        (fun (engine : Engine<'D>, domain : 'D) -> engine.from(over(domain), expr))
    member __.Yield x = x

    [<CustomOperation("select")>]
    member __.Select (over : (Engine<'D> * 'D -> 'R proj), [<ProjectionParameter>] mapping : ('R -> _)) = 
        (fun (engine : Engine<'D>, domain : 'D) -> engine.select(mapping, over(engine, domain)))

    [<CustomOperation("where", MaintainsVariableSpace = true)>]
    member __.Where (over : (Engine<'D> * 'D -> 'R proj), [<ProjectionParameter>] byPredicate : ('R -> bool)) = 
        (fun (engine : Engine<'D>, domain : 'D) -> engine.where(byPredicate, over(engine, domain)))

    [<CustomOperation("delete")>]
    member __.Delete (over : (Engine<'D> * 'D -> 'R table)) = 
        (fun (engine : Engine<'D>, domain : 'D) -> engine.delete(over(engine, domain)))

//    [<CustomOperation("deleteFrom")>]
//    member __.DeleteFrom (over : (Engine<'D> * 'D -> 'R proj), [<ProjectionParameter>] disect : ('R proj -> 'R table)) = 
//        (fun (engine : Engine<'D>, domain : 'D) -> engine.deleteFrom(disect(over(engine, domain))))

    member __.Quote() = ()
    member __.Run(q) = q

let command = new CommandBuilder<MyDomain>()

let ce1 =
    command {
        for a in (fun _d -> _d.As) do
        where (a.Name = "foo")
        select (a.ID)
    }

let ce2 =
    command {
        for b in (fun _d -> _d.Bs) do
        where (b.Name2 = "foo")
        select (b.ID2)
    }

//let ce2 =
//    command {
//        for b in (fun _d -> _d.Bs) do
//        where (b.Name2 = "foo")
//        deleteFrom
//    }

printfn "%A" ce1