namespace TaskTowerSandbox.Sandboxing;

using System.Text.Json;
using System.Text.Json.Serialization;
using Database;
using Domain.JobStatuses;
using Domain.RunHistories;
using Domain.RunHistories.Models;
using Serilog;

public class DoAThing
{
    public sealed record Command(string Data);
        
    public async Task Handle(Command request)
    {
        // await Task.Delay(1000);
        Log.Information("Handled DoAThing with data: {Data}", request.Data);
    }
}

public class DoAnInjectableThing(IDummyLogger logger, TaskTowerDbContext context, PokeApiService pokeApiService)
{
    public sealed record Command(string Data);
    
    public async Task Handle(Command request)
    {
        // await Task.Delay(1000);
        var result = context.RunHistories.FirstOrDefault(x => x.Status == JobStatus.Completed());
        Log.Information("I just read a RunHistory with an ID of {TempId}", result?.Id); 
        
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
