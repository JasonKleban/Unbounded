open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.DerivedPatterns
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.ExprShape
open System

[<NoComparison; NoEquality; Sealed>]
type Table<'T> private () = class end

//type Zero = class end
//type Yield (value) =
//    member __.Value : 'T = value
//type For (table, expr) = 
//    member __.Table : 'T Table = table
//    member __.Expr : ('T -> 'U) = expr
//type Where (element, bool) =
//    member __.element

[<NoComparison; NoEquality>]
type SoFar<'D, 'T, 'Q, 'R> =
    | Zero
    | Yield of value : 'T
    | For of table : ('D -> 'T Table) * expr : ('T -> 'Q) 
    | Where of soFar : SoFar<'D, 'T, 'Q, 'R> * predicate : ('T -> bool)
    | Select of soFar : SoFar<'D, 'T, 'Q, 'R> * selector : ('T -> 'R)
    | Exists of soFar : SoFar<'D, 'T, 'Q, 'R> * predicate : ('T -> bool)
    | Delete of soFar : SoFar<'D, 'T, 'Q, 'R>

type CommandBuilder() =
    member __.For (table, expr) = For (table, expr)
    member __.Zero () = Zero
    member __.Yield x = Yield x
    [<CustomOperation("where",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member __.Where(soFar, [<ProjectionParameter>] predicate) = Where (soFar, predicate)
    [<CustomOperation("select",AllowIntoPattern=true)>]
    member __.Select(soFar, [<ProjectionParameter>] selector) = Select (soFar, selector)
    [<CustomOperation("delete")>] 
    member __.Delete (soFar) = Delete soFar
    [<CustomOperation("exists")>]
    member __.Exists (soFar, [<ProjectionParameter>] predicate) = Exists (soFar, predicate)
//    member __.Quote() = () // <-- I saw a note about using this Quote and Run for 
//    member __.Run(q) = q   //     debugging, but I don't know how it is supposed to work

let command = new CommandBuilder()

(* BUSINESS DOMAIN STUFF *)

module BusinessDomain = 
    open System.Collections.Generic

    type PersonKey = 
        | Auto                  // Unassigned
        | PersonKey of int64    // Can be composite keys
    and Person = {
        ID : PersonKey
        mutable Name : string
        mutable ParentID : PersonKey option
        mutable Birthdate : System.DateTime }

    type ItemKey = 
        | Auto                  
        | PersonKey of int64    
    and Item = {
        ID : ItemKey
        mutable Name : string }

    [<AbstractClass>] 
    type Domain =
        abstract member People : Person Table
        abstract member Items : Item Table

    let invariant1 (domain:Domain) =
        command {
            for person in (fun () -> domain.People) do       
            //for person in domain.People do     
            exists (person.Birthdate.Year < 1900)
        }

    let invariant2 (domain:Domain) =
        command {
            let d = domain
            for item in d.Items do
            exists (item.Name.Contains("RESERVED"))
        }

    let invariants =                        
        dict [                              
            ("invariant1", invariant1) ;    
            ("invariant2", invariant2)      
        ]                                   

[<ReflectedDefinition>]
let cmd1 (domain:BusinessDomain.Domain) = 
    command { 
        for person in domain.People do 
        where (person.Name = "Jason")
        delete }

[<ReflectedDefinition>]
let cmd2 (domain:BusinessDomain.Domain) = 
    command { 
        for person in domain.People do 
        where (person.Name = "Jason")
        select person into selectedPeople        // <-- this version is overly complicated just to test
        for selectedPerson in selectedPeople do  //     on this small domain, but this `select person` 
        delete                                   //     is not type checking.
        (* ... and then continue with 
        selectedPeople, perhaps joining 
        to some other table and inserting 
        into yet other tables *) }
    
// I don't know how quotation reflection and the constructed SoFar objects with their own description of the computation.
// Are they both necessary?  Will they play together?
let BodyOf op =
    match op with
    | Patterns.Call(None, DerivedPatterns.MethodWithReflectedDefinition body, args) -> body
    | Patterns.Lambda (a, Patterns.Call(None, DerivedPatterns.MethodWithReflectedDefinition body, args)) -> body
    | Patterns.Lambda (a, Lambda (b, Patterns.Call(None, DerivedPatterns.MethodWithReflectedDefinition body, args))) -> body
    | e -> e  

printfn "cmd1 is %A" <| BodyOf <@@ cmd1 @@>

printfn "%A" (cmd1)