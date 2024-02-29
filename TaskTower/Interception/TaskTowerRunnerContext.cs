namespace TaskTower.Interception;


/// <summary>
/// A store for the current running job information for accessing the current job id
/// </summary>
public interface ITaskTowerRunnerContext
{
    /// <summary>
    /// The id of the current running job
    /// </summary>
    public Guid JobId { get; set; }
}

/// <inheritdoc/>
public sealed class TaskTowerRunnerContext : ITaskTowerRunnerContext
{
    /// <inheritdoc/>
    public Guid JobId { get; set; }
}