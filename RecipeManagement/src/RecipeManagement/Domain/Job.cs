namespace RecipeManagement.Domain;

using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using Databases;
using Newtonsoft.Json;
using Services;

public class Job : BaseEntity
{
    public string TypeName { get; set; }
    public string MethodName { get; set; }
    [Column(TypeName = "jsonb")]
    public string Parameters { get; set; }
    public JobState State { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string ErrorMessage { get; set; }
}

public enum JobState
{
    Enqueued,
    Processing,
    Succeeded,
    Failed,
    Scheduled // For future use if you want to support scheduled jobs
}
