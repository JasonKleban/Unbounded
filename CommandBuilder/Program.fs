open System.Linq
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.DerivedPatterns
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.ExprShape
open Prototypes
open System

// Extracts the F# expression tree from the quotation and [<ReflectedDefinition>]
let BodyOf op =
    match op with
    | Patterns.Call(None, DerivedPatterns.MethodWithReflectedDefinition body, args) -> body
    | Patterns.Lambda (a, Patterns.Call(None, DerivedPatterns.MethodWithReflectedDefinition body, args)) -> body
    | Patterns.Lambda (a, Lambda (b, Patterns.Call(None, DerivedPatterns.MethodWithReflectedDefinition body, args))) -> body
    | e -> e  

let command = new CommandBuilder()

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
    
[<ReflectedDefinition>]
let cmd1 (domain:Domain) = 
    command {
        for z in (Target (Some domain.People)) do
        where (z.Name <> "Jason")
        z.Name <- "Bob"
        z.Birthdate <- System.DateTime.Now
    }

[<ReflectedDefinition>]
let cmd2 (domain:Domain) = 
    command {
        for z in (Target (Some domain.People)) do
        where (z.Name <> "Jason")
        delete
    }

[<ReflectedDefinition>]
let cmd3 (domain:Domain) = 
    command {
        select { ID = Auto; Name = ""; ParentID = None; Birthdate = new DateTime(1980, 1, 1) }
        insertInto domain.People
    }

printfn "%A" <| BodyOf <@@ cmd1 @@>
printfn "%A" <| BodyOf <@@ cmd2 @@>
printfn "%A" <| BodyOf <@@ cmd3 @@>