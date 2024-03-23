namespace TaskTower.Domain.TaskTowerJob.Dtos;

public sealed record TaskTowerJobWithTagsView
{
    
    public Guid Id { get; set; }
    
    public string? Queue { get; set; }

    public string Status { get; set; }
    
    public string Type { get; set; } = null!;
    
    public string Method { get; set; } = null!;

    public string[] ParameterTypes { get; set; } = Array.Empty<string>();
    
    public string Payload { get; set; } = null!;
    
    public int Retries { get; set; } = 0;
    
    public int? MaxRetries { get; set; }
    
    public DateTimeOffset RunAfter { get; set; }
    
    public DateTimeOffset? RanAt { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? Deadline { get; set; }

    public string JobName { get; set; }
    public string? TagNames { get; set; }
    public string[] Tags => TagNames == null 
        ? Array.Empty<string>() 
        : 
        TagNames.Contains(',') 
            ? TagNames.Split(",") 
            : new[] { TagNames.Trim() };
}