# Docs



# IMAGE

## Simple, reliable & efficient background jobs in .NET - an alternative to HangFire

Task Tower is....

A high level overview of the job flow:

-  A job is be added to your database
- A background service will

## Getting Started

TBD

## Features

- Enqueued Jobs ✅
- Scheduled Jobs ✅
- Recurring Jobs  🚧
- Retries of failed jobs ✅
- Various queue prioritizations ✅
- Job tags  🚧
- Automatic recovery of tasks in the event of a worker crash 🚧
- Timeout handling 🚧
- Deadline handling 🚧
- Uses PostgreSQL's `LISTEN`/`NOTIFY` channels reduce polling frequency enabling immediate message delivery ✅
- Rich, modern Web UI for easy and in depth visibility 🚧
- OTel integration 🚧
- Heartbeat monitoring  🚧

## Who Task Tower is For (and not)

- 
- 

## Options

TBD

> Note that check intervals have a hard coded minimum of 500ms

## How Task Tower Works

1. Jobs are created (e.g. using an `Enqueue`, `Schedule`, etc.)
   1. When a job is inserted and should be processed immediately, it is immediately enqueued using a database trigger
   2. When a job is inserted and should be processed at a later time, Task Tower will poll at at a `JobCheckInterval` to be enqueued
2. Task Tower will check the queue at an interval of `QueueAnnouncementInterval` based on whatever prioritization strategy has been configured and announce jobs for processing using Postges' `pg_notify`
3. Task Tower will be listening for announced jobs and process them

## Queues

There are a various different queuing prioritization strategies that can be used in Task Tower. Jobs with no reconized queue will be considered a lowest priority based on whatever rules apply to the type of prioritization.

> 💡 By default, all jobs will be added to a `default` queue unless otherwise designated

### None (FIFO)

This prioritization strategy will process jobs using no particular prioritization and just behavior in a simple first-in-first-out (FIFO) fashion.

### Alphanumeric

This prioritization strategy will process jobs using an alphanumeric order based on teh queue name. If you are coming from Hangfire and need pairity with their queue behavior, you can lean on this.

Example:

```c#
builder.Services.AddTaskTower(builder.Configuration,x =>
{
    x.QueuePrioritization = QueuePrioritization.AlphaNumeric();
    // ....
});
```

### Index

If you need to create multiple queues and want to prioritize processing of all jobs in one queue over other queues, you can use the `Index` option. This means that jobs will be pulled from the queue sorted in the priory designated in the configuration.

Example:

```c#
builder.Services.AddTaskTower(builder.Configuration,x =>
{
    x.QueuePriorities = new Dictionary<string, int>
    {
        {"critical", 3},
        {"default", 2},
        {"low", 1}
    };
    x.QueuePrioritization = QueuePrioritization.Index();
    // ....
});
```

This example will configure Task Tower to have three queue priorities: **critical**, **default**, and **low** with strict priority. When using the `Index` priority, the queues with higher a priority will be prioritized at the top of the queue for processing.

### Strict

If you need to create multiple queues and need to process all tasks in one queue over other queues, you can use the `Strict` option. Only queues that are given a priority will be recognized and ran when using this strategy. Jobs in unprioritized queues (that are not configured with a priority in the Task Tower config) will not be picked up for processing.

Example:

```c#
builder.Services.AddTaskTower(builder.Configuration,x =>
{
    x.QueuePriorities = new Dictionary<string, int>
    {
        {"critical", 3},
        {"default", 2},
        {"low", 1}
    };
    x.QueuePrioritization = QueuePrioritization.Strict();
    // ....
});
```

This example will configure Task Tower to have three queue priorities: **critical**, **default**, and **low** with strict priority. In strict priority mode, the queues with higher priority is always processed first, and queues with lower priority is processed only if all the other queues with higher priorities are empty.

So in this example, tasks in **critical** queue is always processed first. If **critical** queue is empty, then **default** queue is processed. If both **critical** and **default** queue are empty, then **low** queue is processed.

### Weighted 🚧

**TBD**

The number associated with the queue name is the priority level for the queue.

With this above configuration:

- tasks in **critical** queue will be processed **60%** of the time
- tasks in **default** queue will be processed **30%** of the time
- tasks in **low** queue will be processed **10%** of the time

Example:

```c#
builder.Services.AddTaskTower(builder.Configuration,x =>
{
    x.QueuePriorities = new Dictionary<string, int>
    {
        {"high", 60},
        {"default", 30},
        {"low", 10}
    };
    x.QueuePrioritization = QueuePrioritization.Weighted();
    // ....
});
```



## Tags

You can use the background client to add tags to jobs to allow for easier categorization and filtering of jobs, facilitating  quick identification and management of related tasks. Tags also enhance monitoring and debugging capabilities to let you efficiently track job execution and diagnose issues within specific job  groups.

You can add tags to a job using a veriety of APIs.

```csharp
var command = new DoAThing.Command("this is a tagged job");
var jobId = await client.Enqueue<DoAThing>(x => x.Handle(command));

client.TagJob(jobId, "tag1")
    .TagJob(jobId, "tag2");

await client.TagJobAsync(jobId, "tag3");
await client.TagJobAsync(jobId,  ["tag4"]);

client.TagJob(jobId, "tag5", "tag6");
client.TagJob(jobId, ["tag7", "tag8", "tag9"]);
client.TagJob(jobId,  ["tag10", "tag11"]);
```



## Benchmarks