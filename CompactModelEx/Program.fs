open System.Linq
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.DerivedPatterns
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.ExprShape
     
// This type is a stand-in for the schema to be encountered on the server
// I don't know if an existing Set-like type should be used or some new type should represent tables
type Table<'T> private () =
    member this.Add : 'T -> 'T = failwith "never really executed" 
    interface IQueryable<'T> with
        member this.ElementType: System.Type = failwith "never really executed" 
        member this.Expression: Expressions.Expression = failwith "never really executed"    
        member this.GetEnumerator(): System.Collections.IEnumerator = failwith "never really executed"      
        member this.GetEnumerator(): 'T System.Collections.Generic.IEnumerator = failwith "never really executed"    
        member this.Provider: IQueryProvider = failwith "never really executed" 
     
// Extracts the F# expression tree from the quotation and [<ReflectedDefinition>]
let BodyOf op =
    match op with
    | Patterns.Call(None, DerivedPatterns.MethodWithReflectedDefinition body, args) -> body
    | Patterns.Lambda (a, Patterns.Call(None, DerivedPatterns.MethodWithReflectedDefinition body, args)) -> body
    | Patterns.Lambda (a, Lambda (b, Patterns.Call(None, DerivedPatterns.MethodWithReflectedDefinition body, args))) -> body
    | e -> e     
     
// Models below - consists right now of a single Person model with a 
// self-reference - presumably mapped to some SQL schema.
// Then an ethereal Domain model to provide an idea of the database itself
// These types
     
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
     
// These are the invariants on the domain
// A result of `true` indicates a violation of the constraint
[<ReflectedDefinition>]
module businessRules =
    // uniqueness constraint - hopefully redundant with a PRIMARY KEY constraint
    let People_Unique (d : Domain) = query { 
        for person in d.People do
        join person2 in d.People on (person.ID = person2.ID)
        exists (true) }
     
    // referential integrity - hopefully redundant with a FOREIGN KEY constraint
    let People_ParentExists (d : Domain) = query { 
        for child in d.People do
        where (child.ParentID.IsSome)
        leftOuterJoin parent in d.People on (child.ParentID = Some parent.ID) into parents
        for parent in parents do
        where (box parent = null)
        exists (true) }
     
    let People_ValidName (d : Domain) = query { 
        for person in d.People do
        exists (person.Name.Trim() = "") }
     
    let People_OlderParent (d : Domain) = query { 
        for parent in d.People do
        join child in d.People on (Some parent.ID = child.ParentID)
        exists (child.Birthdate <= parent.Birthdate) }
     
// various operations ...
     
[<ReflectedDefinition>]
let myDad (d : Domain) = // : PersonKey
    d.People.Add({ ID = Auto; Name = "Alex"; ParentID = None; Birthdate = new System.DateTime(1950, 03, 07)}).ID
     
[<ReflectedDefinition>]
let me (d : Domain, parent : PersonKey) = // : PersonKey
    d.People.Add({ ID = Auto; Name = "Pat"; ParentID = Some parent; Birthdate = new System.DateTime(1980, 03, 11)}).ID
     
[<ReflectedDefinition>]
let sibling (d : Domain, parent: PersonKey) = // : PersonKey
    d.People.Add({ ID = Auto; Name = "Sam"; ParentID = Some parent; Birthdate = new System.DateTime(1985, 12, 14)}).ID
     
[<ReflectedDefinition>]
let operation3 (d : Domain) pid = // : unit
    let person = d.People.Where(fun p -> p.ID = pid).Single() // or the query syntax
    person.Name <- "Terry"
    person.Birthdate <- new System.DateTime(1948, 04, 25)
     
// this just prints out the body's expression tree, but we'd instead send them for remote execution
printfn "%A" <| BodyOf <@@ myDad @@>
printfn "%A" <| BodyOf <@@ me @@>
printfn "%A" <| BodyOf <@@ operation3 @@>

(*
-- At runtime, the operations and the invariants are converted to t-sql.  
-- They are also analyzed for relevance of invariants to an operation.
-- `myDad` becomes t-sql with assertions appended

BEGIN TRAN

DECLARE @__ScpID BIGINT

INSERT INTO dbo.People VALUES (Name, Birthdate) SELECT @p1, @p2

SELECT @__ScpID = SCOPE_IDENTITY()

-- Relevant Assertions
-- People_Unique - (natively checked by dbo.People.pk_People)
-- People_ParentExists - (natively checked by dbo.People.fk_People_ParentPersonID)
-- People_ValidName
IF (EXISTS (SELECT 1 FROM dbo.People e1 WHERE e1.Name = '' AND e1.ID = @__ScpID)) BEGIN ROLLBACK; RETURN; END

-- Irrelevant Assertions
-- People_OlderParent (with inserted item as e1)
-- People_OlderParent (with inserted item as e2)

COMMIT
*)