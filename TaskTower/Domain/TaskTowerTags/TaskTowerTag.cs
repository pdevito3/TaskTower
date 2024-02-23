namespace TaskTower.Domain.TaskTowerTags;

using TaskTowerJob;

public class TaskTowerTag
{
    /// <summary>
    /// The id of the job
    /// </summary>
    public Guid JobId { get; private set; }
    internal TaskTowerJob Job { get; } = null!;

    /// <summary>
    /// The name of the tag
    /// </summary>
    public string Name { get; private set; } = null!;
    
    public static TaskTowerTag Create(Guid jobId, string name)
    {
        var tag = new TaskTowerTag();
        
        if (jobId == Guid.Empty)
        {
            throw new ArgumentException("JobId cannot be empty", nameof(jobId));
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tag name cannot be empty", nameof(name));
        }
        
        tag.JobId = jobId;
        tag.Name = name;
        return tag;
    }

    private TaskTowerTag() { } // EF Core
}