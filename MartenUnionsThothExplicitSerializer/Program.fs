open System
open System.Collections.Generic
open Marten
open Marten.Schema.Identity
open Thoth.Json.Net
open Weasel.Core
open Weasel.Postgresql

type RegisteredEvent = { RegisteredOn: DateTime }
type UserEvent = Registered of RegisteredEvent

type ThothSerializer() =
  interface ISerializer with
    member this.Casing = Casing.CamelCase
    member this.EnumStorage = EnumStorage.AsInteger
    member this.FromJson(stream) = failwith "todo"
    member this.FromJson<'T>(reader: System.Data.Common.DbDataReader, index: int) : 'T = failwith "todo"
    member this.FromJson(``type``: System.Type, stream: System.IO.Stream) : obj = failwith "todo"
    member this.FromJson(``type``, reader, index) = failwith "todo"
    member this.FromJsonAsync(stream, cancellationToken) = failwith "todo"

    member this.FromJsonAsync<'T>
      (
        reader: System.Data.Common.DbDataReader,
        index: int,
        cancellationToken: System.Threading.CancellationToken
      ) : System.Threading.Tasks.ValueTask<'T> =
      failwith "todo"

    member this.FromJsonAsync
      (
        ``type``: System.Type,
        stream: System.IO.Stream,
        cancellationToken: System.Threading.CancellationToken
      ) : System.Threading.Tasks.ValueTask<obj> =
      failwith "todo"

    member this.FromJsonAsync(``type``, reader, index, cancellationToken) = failwith "todo"
    member this.ToCleanJson(document) = Encode.Auto.toString (4, document)

    member this.ToJson(document) =
      let event = document :?> UserEvent
      Encode.Auto.toString (0, event)

    member this.ToJsonWithTypes(document) = Encode.Auto.toString (0, document)
    member this.ValueCasting = ValueCasting.Strict

let save (streamId: Guid) events =
  use store =
    DocumentStore.For
      (fun options ->
        let connectionString =
          sprintf "host=%s;port=%i;database=%s;username=%s;password=%s" "localhost" 5432 "events" "marten" "123456"

        options.Connection(connectionString)
        options.AutoCreateSchemaObjects <- AutoCreate.All
        options.Serializer(ThothSerializer())
        options.Events.AddEventType(typeof<RegisteredEvent>))

  use session = store.OpenSession()

  let eventData = events |> List.toArray

  printfn $"%s{streamId.ToString()}"
  printfn $"%A{eventData}"

  session.Events.Append(streamId, eventData)
  |> ignore

  session.SaveChangesAsync()
  |> Async.AwaitTask
  |> Async.RunSynchronously

let registered: UserEvent =
  Registered { RegisteredOn = DateTime.Today }

let id = CombGuidIdGeneration.NewGuid()

save id [ registered ]
