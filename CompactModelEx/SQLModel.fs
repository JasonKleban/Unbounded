module SQLModel

open Microsoft.FSharp.Quotations

type sqlValue = 
  | TinyInt of byte
  | SmallInt of int16
  | Int of int32
  | BigInt of int64
  | DateTime of System.DateTime

module SqlValue =
  let toSql = function
    | TinyInt(value) -> string(value)
    | SmallInt(value) -> string(value)
    | Int(value) -> string(value)
    | BigInt(value) -> string(value)
    | DateTime(value) -> string(value)
  let fromExpr(o:obj) = 
    match o with
    | :? int as v -> Int(v)
    | :? int16 as v -> SmallInt(v)
    | :? int64 as v -> BigInt(v)
    | :? byte as v -> TinyInt(v)
    | :? System.DateTime as v -> DateTime(v)
    | _ -> raise (System.Exception(sprintf "Unknown type: %s" (o.GetType().Name)))

type SqlType = 
  | Datetime 
  | Varchar of MaxLength:uint32
  | Nullable of InnerType:SqlType

type Parameter = Parameter of Name:string * Type:SqlType

type sqlBinaryOperation = Add | Multiply | Subtract | Divide

module SqlBinaryOperation =
  let toSql = function
  | Add -> "+"
  | Multiply -> "*"
  | Subtract -> "-"
  | Divide -> "-"

type sqlExpr = 
  | Var of Name:string
  | Value of Value:sqlValue
  | BinaryOp of Op:sqlBinaryOperation * Left:sqlExpr * Right:sqlExpr

module SqlExpr =
  let rec toSQL = function
    | Var(name) -> sprintf "@%s" name
    | Value(v) -> SqlValue.toSql(v)
    | BinaryOp(op, left, right) -> 
      let left = (toSQL left)
      let op = (op |> SqlBinaryOperation.toSql)
      let right = (toSQL right)
      sprintf "(%s %s %s)" left op right
  let rec fromExpr = function
    | Patterns.Value(v, t) -> Value(SqlValue.fromExpr v)
    | Patterns.Var(v) -> Var(v.Name)
    | Patterns.Call(_,m,[left;right]) ->
      match m.Name with
      | "op_Addition" -> BinaryOp(Add, (fromExpr left), (fromExpr right))
      | "op_Multiply" -> BinaryOp(Multiply, (fromExpr left), (fromExpr right))
      | "op_Subtraction" -> BinaryOp(Subtract, (fromExpr left), (fromExpr right))
      | "op_Division" -> BinaryOp(Divide, (fromExpr left), (fromExpr right))
      | _ -> raise(System.Exception(sprintf "Operation %s not supported" m.Name))
    | x -> raise(System.Exception(sprintf "Pattern %A cannot be translated" x))

type sqlElement = 
  | SqlExpr of Expr:sqlExpr
  | SqlFunction of Name:string * Parameters:Parameter list * Body:sqlExpr

