﻿namespace Infrastructure

open Microsoft.WindowsAzure.Storage
open CosmoStore.TableStorage
open Newtonsoft.Json.Linq



module EventStore =

    open System
    open FSharp.Control.Tasks.V2
    open Newtonsoft.Json.Linq
    open Domain.Common
    open Dtos.Common
    open CosmoStore
    open CosmoStore.TableStorage
    open System.Threading.Tasks

    

    [<CLIMutable>]
    type EventEntity = {
        Id:string
        EventType:string
        Data:string
        Version:int
    }

        


    let private toEventData aggregate (data:IEvent) =
        let jToken = JToken.FromObject(data)
        {
            EventWrite.Id = Guid.NewGuid()
            CorrelationId = None
            CausationId = None
            Name = data.EventType
            Data = jToken 
            Metadata = None
        }


    let storeEvents (getEventStore:unit -> Task<EventStore<JToken,int64>>) aggregate id (events:IEvent list) =
        task {
            let! eventStore = getEventStore ()
            let streamId = sprintf "%s-%s" aggregate id
                
            try 
                let batchedEvents =
                    events
                    |> List.map (fun i -> i |> toEventData aggregate)
                    |> List.chunkBySize 99

                for batch in batchedEvents do
                    let! x = eventStore.AppendEvents streamId ExpectedVersion.Any batch
                    ()

                return () |> Ok
            with
            | _ as e ->
                return e |> InfrastructureError |> Error
        }


    let private readAllEvents (getEventStore:unit -> Task<EventStore<JToken,int64>>) eventConverter aggregate id =
        task {
            let! eventStore = getEventStore ()
            let streamId = sprintf "%s-%s" aggregate id
            try
                let! result = EventsReadRange.AllEvents |> eventStore.GetEvents streamId
                
                let events =
                    result
                    |> Seq.map (eventConverter)
                    |> Seq.toList
                return events |> Ok
            with
            | _ as e ->
                return e |> InfrastructureError |> Error
                
        }


    let private readEventsSpecificVersion (getEventStore:unit -> Task<EventStore<JToken,int64>>) eventConverter aggregate id version =
        task {
            let! eventStore = getEventStore ()
            let streamId = sprintf "%s-%s" aggregate id
            try
                let! result = EventsReadRange.FromVersion(version) |> eventStore.GetEvents streamId
                let events =
                    result
                    |> Seq.map (eventConverter)
                    |> Seq.toList
                return events |> Ok
            with
            | _ as e ->
                return e |> InfrastructureError |> Error
                
        }

    let private readAllStreams (getEventStore:unit -> Task<EventStore<JToken,int64>>) aggregate =
        task {
            let! eventStore = getEventStore ()
            try
                let! streams = StreamsReadFilter.StartsWith(aggregate) |> eventStore.GetStreams
                return streams |> Ok
            with
            | _ as e ->
                return e |> InfrastructureError |> Error
        
        }
    
    
   

    module User =

        open Dtos.User.Events
        open System.Threading.Tasks
        


        let private eventConverter (event:EventRead<JToken,int64>) : Domain.User.Event * int64 =
            match event.Name with
            | x when x = nameof UserCreated ->
                let e = event.Data.ToObject<UserCreated>()
                e |> toDomain, event.Version
            | x when x = nameof  UserDeleted ->
                let e = event.Data.ToObject<UserDeleted>()
                e |> toDomain, event.Version
            | x when x = nameof  EMailChanged ->
                let e = event.Data.ToObject<EMailChanged>()
                e |> toDomain, event.Version
            | x when x = nameof  NameChanged ->
                let e = event.Data.ToObject<NameChanged>()
                e |> toDomain, event.Version
            | x when x = nameof  PasswordChanged ->
                let e = event.Data.ToObject<PasswordChanged>()
                e |> toDomain, event.Version
            | x when x = nameof  AddedToGroup ->
                let e = event.Data.ToObject<AddedToGroup>()
                e |> toDomain, event.Version
            | x when x = nameof  RemovedFromGroup ->
                let e = event.Data.ToObject<RemovedFromGroup>()
                e |> toDomain, event.Version
            | _ ->
                failwith "can not convert event"


        let readEvents (getEventStore:unit -> Task<EventStore<JToken,int64>>) aggregate id : Task<Result<(Domain.User.Event * int64) list,Errors>> =
            readAllEvents getEventStore eventConverter aggregate id


        let readEventsStartSpecificVersion (getEventStore:unit -> Task<EventStore<JToken,int64>>) aggregate id version : Task<Result<(Domain.User.Event * int64) list,Errors>> =
            readEventsSpecificVersion getEventStore eventConverter aggregate id version 


        let readAllUserStreams (getEventStore:unit -> Task<EventStore<JToken,int64>>) aggregate : Task<Result<Stream<int64> list,Errors>> =
            readAllStreams getEventStore aggregate

        
        
            
                    
                
        


         