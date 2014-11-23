open System

type Column = {
  Table : string
  Name : string
}

type SqlLiteral = 
  | Number of int
  | Str of string
  | Null

type BoolExpressionLeaf =
  | Literal of SqlLiteral
  | ColumnReference of Column

type BooleanExpression = 
  | And of BooleanExpression list
  | Or of BooleanExpression list
  | Not of BooleanExpression
  | Equals of left : BoolExpressionLeaf * right : BoolExpressionLeaf
  | LessThan of left : BoolExpressionLeaf * right : BoolExpressionLeaf

type SqlQuery = {
  Selects : Column list
  Where : BooleanExpression
}

type SqlUpdate = {
  Update : Column * SqlLiteral
  Where : BooleanExpression
}

type Constraint = BooleanExpression

let query (selectList, whereExpression) = { Selects = selectList; Where = whereExpression }
let update (update, whereExpression) = { Update = update; Where = whereExpression }

let column table name = { Table = table; Name = name }

let peopleTable = column "people"

let name = peopleTable "name"
let age = peopleTable "age"
let dead = peopleTable "is_dead"

let update1 = update ((dead, Number 1), Equals(ColumnReference name, Literal <| Str "John Doe"))

let ageConstraint = 
  And [
    LessThan(ColumnReference age, Literal <| Number 30)
    Equals(ColumnReference dead, Literal <| Number 0)
  ]

let rec getExprColumnRefs = function
  | And constraints
  | Or constraints -> List.collect getExprColumnRefs
  | Not constraint -> getExprColumnRefs constraint
  | Equals(ColumnReference col1, ColumnReference col2) -> [ col1; col2 ]
  | Equals(ColumnReference col, _)
  | Equals(_, ColumnReference col) -> [ col ]
  | LessThan(ColumnReference col1, ColumnReference col2) -> [ col1; col2 ]
  | LessThan(ColumnReference col, _)
  | LessThan(_, ColumnReference col) -> [ col ]

let constraintApplies query constraint = 
  let queryColumns = seq { yield! query.Selects; yield! getExprColumnRefs query.Where } |> Set.ofSeq
  let constraintColumns = getExprColumnRefs constraint |> Set.ofSeq

  Set.intersect queryColumns constraintColumns |> (not << Set.isEmpty)