open System
open Thoth.Json.Net

type LeadType = { CompanyName: string }

type CustomerType =
  { CompanyName: string
    FirstPurchase: DateTime }

type CustomerRelation =
  | Lead of LeadType
  | Customer of CustomerType

let customer: CustomerRelation =
  Customer
    { CompanyName = "PDMLab"
      FirstPurchase = DateTime.Today }

let serializeObject (relation) =
  let obj = relation :> Object
  Encode.Auto.toString(2, obj)
 
let json = serializeObject customer
printfn $"%s{json}"

let print (relation: CustomerRelation) =
  match relation with
  | Customer customerType -> printfn $"first purchase date: %A{customerType.FirstPurchase}"
  | Lead lead -> printfn "lead"
