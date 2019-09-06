namespace Farmer

type Value =
    | Literal of string
    | Parameter of string
    | Variable of string
    member this.Value =
        match this with
        | Literal l -> l
        | Parameter p -> sprintf "parameters('%s')" p
        | Variable v -> sprintf "variables('%s')" v
    member this.QuotedValue =
        match this with
        | Literal l -> sprintf "'%s'" l
        | Parameter p -> sprintf "parameters('%s')" p
        | Variable v -> sprintf "variables('%s')" v
    member this.Command =
        match this with
        | Literal l ->
            l
        | Parameter _
        | Variable _ ->
            sprintf "[%s]" this.Value

[<AutoOpen>]
module ExpressionBuilder =
    let private escaped = function
        | Literal x -> sprintf "'%s'" x
        | x -> x.Value
    let command = sprintf "[%s(%s)]"
    let concat (elements:Value list) =
        elements
        |> List.map escaped
        |> String.concat ", "
        |> command "concat"
    let toLower =
        escaped >> command "toLower"

namespace Farmer.Internal

open Farmer

type Expressions =
    | Concat of Value list
    | ToLower of Value

type WebAppExtensions = AppInsightsExtension
type AppInsights =
    { Name : Value
      Location : Value
      LinkedWebsite: Value }
type StorageAccount =
    { Name : Value
      Location : Value
      Sku : Value }
type Dependency =
    | AppInsightsDependency
    | StorageDependency
    | ServerFarmDependency
type WebApp =
    { Name : Value
      AppSettings : List<string * Value>
      Extensions : WebAppExtensions Set
      Dependencies : (Dependency * Value) list }
type ServerFarm =
    { Name : Value
      Location : Value
      Sku:Value
      WebApps : WebApp list }
type TemplateItem =
    | AppInsights of AppInsights
    | StorageAccount of StorageAccount
    | ServerFarm of ServerFarm

namespace Farmer

open Farmer.Internal

type ArmTemplate =
    { Parameters : string list
      Variables : (string * string) list
      Outputs : (string * Value) list
      Resources : TemplateItem list }