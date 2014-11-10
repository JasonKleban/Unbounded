namespace Prototypes

type ITree<'a> =
  abstract Value : 'a
  abstract Children: seq<ITree<'a>>

type Tree<'a>(value:'a, getChildren:ITree<'a> -> seq<ITree<'a>>) =
  member this.Value = value
  member this.Children = getChildren this
  interface ITree<'a> with
    member this.Value = value
    member this.Children = getChildren this

type ProjectedTree<'a,'b>(tree:ITree<'a>, f:'a -> 'b) =
  member this.Value = f tree.Value
  member this.Children = seq { for child in tree.Children do 
                               yield ProjectedTree(child, f) :> ITree<'b> }
  interface ITree<'b> with
    member this.Value = this.Value
    member this.Children = this.Children

module Tree =
  let root (value:'a) = Tree(value, fun v -> Seq.empty<ITree<'a>>) :> ITree<'a>
  let expand (f:'a -> seq<'a>) (tree:ITree<'a>) = 
    let rec getSub (node:ITree<'a>) = 
      node.Value |> f |> Seq.map (fun item -> Tree(item, getSub) :> ITree<'a>)
    Tree(tree.Value, getSub) :> ITree<'a>
  let rec map (f:'a -> 'b) (tree:ITree<'a>) : ITree<'b> =
    ProjectedTree(tree, f) :> ITree<'b>
  let rec filter (f:'a -> bool) (tree:ITree<'a>) =
    let getSub (node:ITree<'a>) =
      node.Children |> Seq.filter (fun node -> f node.Value)
    Tree(tree.Value, getSub) :> ITree<'a>
  let rec maxDepth (max:int) (tree:ITree<'a>) : ITree<'a> =
    if max < 1 then root tree.Value
    else 
      let getSub (node:ITree<'a>) =
        node.Children |> Seq.map (maxDepth (max - 1))
      Tree(tree, getSub) :> ITree<'a>

//    { new ITree<'a> with
//        member this.Value = tree.Value
//        member this.Children = 
//          if max <= 1 then Seq.empty
//          else seq { for c in tree.Children do
//                     yield maxDepth (max - 1) tree }
//    }
    

open System
open System.Runtime.CompilerServices

[<Extension>]
type TreeExtensions() =
  [<Extension>]
  static member Expand (tree:ITree<'a>) (f:System.Func<'a,seq<'a>>) = 
    tree |> Tree.expand f.Invoke 
  [<Extension>]
  static member Where (tree:ITree<'a>) (pred:Func<'a,bool>) =
    tree |> Tree.filter pred.Invoke
  [<Extension>]
  static member Select (tree:ITree<'a>) (f:Func<'a,'b>) =
    tree |> Tree.map f.Invoke
  [<Extension>]
  static member MapDepth (tree:ITree<'a>) (max:int) =
    tree |> Tree.maxDepth max
