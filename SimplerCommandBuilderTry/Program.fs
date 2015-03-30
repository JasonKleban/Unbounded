open System.Linq
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.DerivedPatterns
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.ExprShape
open System

(* LIBRARY STUFF *)

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
    [<CustomOperation("select",AllowIntoPattern=true)>]  
    member __.Select(target:Target<'T,'Q>, selector:('T -> 'U)) : Target<'U,'Q> = Target<'U,'Q> None
    [<CustomOperation("where",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member __.Where(target:Target<'T,'Q>, predicate:('T -> bool)) : Target<'T,'Q> = Target<'T,'Q> None 
    [<CustomOperation("insertInto" )>] 
    member __.Insert (source: Target<'T,'Q>, target:(unit -> Table<'T>)) = ()  
    [<CustomOperation("delete")>] 
    member __.Delete (target:Target<'T,'Q>) = ()
    [<CustomOperation("exists")>]
    member __.Exists (target:Target<'T,'Q>, predicate:('T -> bool)) : bool = true
    member __.Quote() = ()
    member __.Run(q) = q

let command = new CommandBuilder()

(* BUSINESS DOMAIN STUFF *)

module BusinessDomain = 
    type PersonKey = 
        | Auto                  // Unassigned
        | PersonKey of int64    // Can be composite keys
    and Person = {
        ID : PersonKey
        mutable Name : string
        mutable ParentID : PersonKey option
        mutable Birthdate : System.DateTime }

    [<AbstractClass>] 
    type Domain =
        abstract member People : Person Table

    let invariant1 (domain:Domain) = (* Somehow invariants such as these will be "registered" with the library as needing to be enforced *)
        command {
            for z in (Target (Some domain.People)) do
            exists (z.Birthdate < 1900)
        }

(* DOMAIN-WISE USER OPERATIONS *)

[<ReflectedDefinition>]
let cmd1 (domain:BusinessDomain.Domain) = 
    command {
        for z in (Target (Some domain.People)) do
        where (z.Name <> "Jason")
        z.Name <- "Bob"
        z.Birthdate <- System.DateTime.Now
    }

[<ReflectedDefinition>]
let cmd2 (domain:BusinessDomain.Domain) = 
    command {
        for z in (Target (Some domain.People)) do
        where (z.Name <> "Jason")
        delete
    }

[<ReflectedDefinition>]
let cmd3 (domain:BusinessDomain.Domain) = 
    command {
        select { ID = Auto; Name = ""; ParentID = None; Birthdate = new DateTime(1980, 1, 1) }
        insertInto domain.People
    }

(* Somehow the operations above get emitted as the necessary SQL code, multiple statements with intermediate 
   table-variables etc., as needed INCLUDING the library-selected and library-narrowed invariant assertions
 *)

let BodyOf op =
    match op with
    | Patterns.Call(None, DerivedPatterns.MethodWithReflectedDefinition body, args) -> body
    | Patterns.Lambda (a, Patterns.Call(None, DerivedPatterns.MethodWithReflectedDefinition body, args)) -> body
    | Patterns.Lambda (a, Lambda (b, Patterns.Call(None, DerivedPatterns.MethodWithReflectedDefinition body, args))) -> body
    | e -> e  

printfn "%A" <| BodyOf <@@ cmd1 @@>
printfn "%A" <| BodyOf <@@ cmd2 @@>
printfn "%A" <| BodyOf <@@ cmd3 @@>