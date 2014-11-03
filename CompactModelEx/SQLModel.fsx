#load "SQLModel.fs"

open SQLModel
open SQLModel.SqlExpr

let test =
  let y = 1
  [
    Var("x") |> toSQL;
    Value(Int(2)) |> toSQL;
    <@ 2 + 2 @> |> fromExpr |> toSQL;
    <@@ (y + 2) * y / 3 - 2 @@> |> fromExpr |> toSQL;
  ]

