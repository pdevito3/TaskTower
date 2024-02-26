namespace TaskTower.Interception;


public record ErrorDetails(string? Message, string? Details, DateTimeOffset? OccurredAt);