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

let serialize (relation) = Encode.Auto.toString (2, relation)

let res = serialize customer
printfn $"%s{res}"

let existingCustomer =
  """["Customer",{"CompanyName":"Acme, Inc.","FirstPurchase":"2022-03-08T00:00:00+01:00"}]"""

let result =
  Decode.Auto.fromString<CustomerRelation> existingCustomer

let print (relation: CustomerRelation) =
  match relation with
  | Customer customerType -> printfn $"first purchase date: %A{customerType.FirstPurchase}"
  | Lead lead -> printfn "lead"

match result with
| Ok relation -> print relation |> ignore
| Error e -> printfn $"%s{e}"
