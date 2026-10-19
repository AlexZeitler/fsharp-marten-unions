# F# Discriminated Unions with Marten Event Store

## What's happening here?

[Marten](https://martendb.io/) is an Event Store and Document database build for .NET on top of Postgres.

Marten works largely with F#, but not if the events are cases of a union type.

The issue is described and discussed [here](https://github.com/JasperFx/marten/issues/1283).

This repository summarizes my attempts to solve the problem so far.

## Solutions / Findings

### The events and the serializer

The events look like this:

```f#
type RegisteredEvent = { RegisteredOn: DateTime }
type UserEvent = Registered of RegisteredEvent
```

```f#
let registered: UserEvent =
Registered { RegisteredOn = DateTime.Today }
```

### Workaround 

I have come up with a workaround based on explicitly casting the union cases to `IEvent`.

This can be seen in `MartenUnionsWorkaround/Program.cs`

`WrappedEvent<'T>(event: 'T)` implements `IEvent`.

