namespace Prototypes

open System
open System.Linq
open System.Collections
open System.Collections.Generic
open Microsoft.FSharp
open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.Operators
open Microsoft.FSharp.Quotations

[<NoComparison; NoEquality; Sealed>]
type Table<'T> private () =
    class end

[<NoComparison; NoEquality; Sealed>]
type Target<'T, 'Q> (target:Table<'T> option) = 
    member __.Target = target
      
type CommandBuilder() =
    member __.For (target:Target<'T,'Q>, body: 'T -> Target<'Result,'Q2>) : Target<'Result,'Q> = Target None
    member __.Zero () = Target None
    member __.Yield (x:'T) = Target<'T,'Q> None    
    member __.Select(target:Target<'T,'Q>, selector:('T -> 'U)) : Target<'U,'Q> = Target<'U,'Q> None
    member __.Where(target:Target<'T,'Q>, predicate:('T -> bool)) : Target<'T,'Q> = Target<'T,'Q> None
    member __.Insert (source: Target<'T,'Q>, target:(unit -> Table<'T>)) = ()
    member __.Delete (target:Target<'T,'Q>) = ()