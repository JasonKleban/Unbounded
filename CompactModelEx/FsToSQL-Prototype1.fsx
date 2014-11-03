open System.Linq
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.DerivedPatterns
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.ExprShape

module FsToSQL = 
  let template (e:Expr) = 
    match e with
    | Patterns.AddressOf(x) -> ""
    | Patterns.AddressSet(e) -> ""
    | Patterns.Application(x, y) -> ""
    | Patterns.Call(expr, methodInfo, list) -> ""
    | Patterns.Coerce(expr, t) -> ""
    | Patterns.DefaultValue(t) -> ""
    | Patterns.FieldGet(expr, field) -> ""
    | Patterns.FieldSet(expr, field, value) -> ""
    | Patterns.ForIntegerRangeLoop(var, e1, e2, e3) -> ""
    | Patterns.IfThenElse(test, ifTrue, ifFalse) -> ""
    | Patterns.Lambda(var, expr) -> ""
    | Patterns.Let(var, expr, body) -> ""
    | Patterns.LetRecursive(bindings, body) -> ""
    | Patterns.NewArray(t, exprList) -> ""
    | Patterns.NewDelegate(t, vars, expr) -> ""
    | Patterns.NewObject(constr, args) -> ""
    | Patterns.NewRecord(t, args) -> ""
    | Patterns.NewTuple(args) -> ""
    | Patterns.NewUnionCase(constr, args) -> ""
    | Patterns.PropertyGet(expr, prop, args) -> ""
    | Patterns.PropertySet(expr, prop, args, value) -> ""
    | Patterns.Quote(expr) -> ""
    | Patterns.Sequential(expr1, expr2) -> ""
    | Patterns.TryFinally(tryBody, finalBody) -> ""
    | Patterns.TryWith(tryBody, var1, catch1, var2, catch2) -> ""
    | Patterns.TupleGet(expr, index) -> ""
    | Patterns.TypeTest(expr, t) -> ""
    | Patterns.UnionCaseTest(expr, constToTest) -> ""
    | Patterns.Value(value, t) -> ""
    | Patterns.Var(var) -> ""
    | Patterns.VarSet(var, assignmentExpr) -> ""
    | Patterns.WhileLoop(testExpr, bodyExpr) -> ""
    | e -> raise (new System.Exception(sprintf "Error translating type: %s" (e.ToString())))

  let rec (|MultiParamLambda|_|) (e:Expr) =
    let rec MatchLambdas (parameters:Var list) (e:Expr) =
      match e with
      | Patterns.Lambda(var, expr) -> MatchLambdas (var::parameters) expr
      | _ -> (parameters, e)
    match e with
    | Patterns.Lambda(var, expr) -> Some(MatchLambdas [var] expr)
    | _ -> None

  let toSQL (e:Expr) = 
    match e with
    | MultiParamLambda(parameters, body) -> 
      sprintf ""
    | e -> raise (new System.Exception(sprintf "Error translating type: %s" (e.ToString())))

module Example1 =
  type BandMember = { Name:string; Position:string; DOB:System.DateTime }
  let george = { Name="George Clinton"; Position="bandleader"; DOB=new System.DateTime(1941, 7, 22) }
  let add x y = x + y
  