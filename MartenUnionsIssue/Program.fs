open System
open System.Collections.Generic
open Marten
open Marten.Schema.Identity
open Microsoft.FSharp.Reflection
open Weasel.Postgresql

type RegisteredEvent = { RegisteredOn: DateTime }
type UserEvent = Registered of RegisteredEvent

type WrappedEvent<'T>(event: 'T) =
  let mutable headers = Dictionary<string, obj>()

  interface Events.IEvent<'T> with
    member self.Data = event

  interface Events.IEvent with
    member self.Data = event :> obj
    member val DotNetTypeName = null with get, set
    member val EventType = null
    member val EventTypeName = null with get, set
    member val Id = Guid.Empty with get, set
    member val Sequence = int64 (0) with get, set
    member val StreamId = Guid.Empty with get, set
    member val StreamKey = null with get, set
    member val TenantId = null with get, set
    member val Timestamp = DateTimeOffset.MinValue with get, set
    member val Version = int64 (0) with get, set
    member val CorrelationId = String.Empty with get, set
    member val CausationId = String.Empty with get, set
    member val IsArchived = false with get, set

    member this.Headers
      with get () = headers
      and set v = headers <- v

    member this.SetHeader(key, value) = headers.Add(key, value)
    member this.GetHeader(key) = headers.[key]

let MapToSubtype subtype =
  match subtype with
  | Registered registeredCustomer -> WrappedEvent registeredCustomer :> Events.IEvent

let store =
  DocumentStore.For
    (fun options ->
      let connectionString =
        sprintf "host=%s;port=%i;database=%s;username=%s;password=%s" "localhost" 5432 "events" "marten" "123456"

      options.Connection(connectionString)
      options.AutoCreateSchemaObjects <- AutoCreate.All
      options.Events.AddEventType(typeof<RegisteredEvent>))

let save (streamId: Guid) events =
  use session = store.OpenSession()

  let eventData = events |> List.toArray

  printfn "Events to append:"
  printfn $"StreamId: %s{streamId.ToString()}"
  printfn $"Events: %A{eventData}"

  session.Events.Append(streamId, eventData)
  |> ignore

  session.SaveChangesAsync()
  |> Async.AwaitTask
  |> Async.RunSynchronously

let getCaseMap<'t> () =
  FSharpType.GetUnionCases(typeof<'t>)
  |> Seq.map
       (fun unionCase ->
         let typ =
           let property = unionCase.GetFields() |> Seq.exactlyOne
           property.PropertyType

         let create data =
           FSharpValue.MakeUnion(unionCase, [| data |]) :?> 't

         typ.Name, create)
  |> Map

let load (streamId: Guid) =
  use session = store.OpenSession()
  let stream = session.Events.FetchStream(streamId)
 
  let events = stream |> Seq.toList

  events[0].Data


let registered: UserEvent =
  Registered { RegisteredOn = DateTime.Today }

let id = CombGuidIdGeneration.NewGuid()

save id [ registered ]
let userEvents = load id
printfn "Loaded events:"
printfn $"%A{userEvents}"
