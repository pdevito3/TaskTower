namespace TaskTowerSandbox.Sandboxing;

using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using TaskTower.Database;
using TaskTower.Domain.JobStatuses;
using TaskTower.Interception;

public class DoAThing
{
    public sealed record Command(string Data);
        
    public async Task Handle(Command request)
    {
        // await Task.Delay(1000);
        Log.Information("Handled DoAThing with data: {Data}", request.Data);
    }
}



public class DoAContextualizerThing(IJobContextAccessor jobContextAccessor)
{
    public sealed record Command(string? User) : IJobWithUserContext;
    
    public async Task Handle(Command request)
    {
        Log.Information("Handled DoAContextualizerThing with a user from the param as: {RequestUser} and from the context as: {UserContextUser} with an Id of {Id} with this noteworth thing: {Note}", 
            request.User, 
            jobContextAccessor?.UserContext?.User,
            jobContextAccessor?.UserContext?.UserId,
            jobContextAccessor?.UserContext?.NullableNote);

    }
}

public class DoAnInjectableThing(IDummyLogger logger, PokeApiService pokeApiService, ITaskTowerRunnerContext context)
{
    public sealed record Command(string Data);
    
    public async Task Handle(Command request)
    {
        // await Task.Delay(1000);
        // var result = context.RunHistories.FirstOrDefault(x => x.Status == JobStatus.Completed());
        // Log.Information("I just read a RunHistory with an ID of {TempId}", result?.Id); 
        
        Log.Information("I am running a job with an Id of {Id} that I got from context", context.JobId);
        
        var pokemon = await pokeApiService.GetRandomPokemonAsync();
        Log.Information("I just read a Pokemon with an ID of {PokemonId} and content of {Name}", pokemon.Item1, pokemon.Item2);
        
        logger.Log($"Handled DoAnInjectableThing with data: {request.Data}");
    }
}

public class DoADefaultThing
{
    public sealed record Command(string Data);
        
    public async Task Handle(Command request)
    {
        // await Task.Delay(1000);
        Log.Information("Handled DoAThing with data: {Data}", request.Data);
    }
}

public class DoACriticalThing
{
    public sealed record Command(string Data);
        
    public async Task Handle(Command request)
    {
        // await Task.Delay(1000);
        Log.Information("Handled DoAThing with data: {Data}", request.Data);
    }
}

public class DoALowThing
{
    public sealed record Command(string Data);
        
    public async Task Handle(Command request)
    {
        // await Task.Delay(1000);
        Log.Information("Handled DoAThing with data: {Data}", request.Data);
    }
}

public class DoAPossiblyFailingThing
{
    public sealed record Command(string Data);
        
    public async Task Handle(Command request)
    {
        var success = new Random().Next(0, 100) < 70;
        if (!success)
        {
            throw new Exception("Failed");
        }
        
        Log.Information("Handled DoAPossiblyFailingThing with data: {Data}", request.Data);
        if (request.Data == "fail")
        {
            throw new Exception("Failed");
        }
    }
}

public class DoASynchronousThing
{
    public sealed record Command(string Data);
    
    public void Handle(Command command)
    {
        // Simulate work
        Console.WriteLine($"Handled DoASynchronousThing with data: {command.Data}");
    }
}

public class DoASlowThing
{
    public sealed record Command(string Data);
    
    public async Task Handle(Command request)
    {
        await Task.Delay(15000);
        Log.Information("Handled DoALongDelayThing with data: {Data}", request.Data);
    }
}
