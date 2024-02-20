namespace TaskTowerSandbox.Domain.JobStatuses;

using System.ComponentModel.DataAnnotations;
using Ardalis.SmartEnum;

public class JobStatus : ValueObject
{
    private JobStatusEnum _status;
    public string Value
    {
        get => _status.Name;
        private set
        {
            if (!JobStatusEnum.TryFromName(value, true, out var parsed))
                throw new ValidationException($"Invalid Status. Please use one of the following: {string.Join("", "", ListNames())}");

            _status = parsed;
        }
    }
    
    public JobStatus(string value)
    {
        Value = value;
    }

    public bool IsPending() => Value == Pending().Value;
    public bool IsEnqueued() => Value == Enqueued().Value;
    public bool IsProcessing() => Value == Processing().Value;
    public bool IsCompleted() => Value == Completed().Value;
    public bool IsFailed() => Value == Failed().Value;
    public bool IsDead() => Value == Dead().Value;
    public static JobStatus Of(string value) => new JobStatus(value);
    public static implicit operator string(JobStatus value) => value.Value;
    public static List<string> ListNames() => JobStatusEnum.List.Select(x => x.Name).ToList();
    public static JobStatus Pending() => new JobStatus(JobStatusEnum.Pending.Name);
    public static JobStatus Enqueued() => new JobStatus(JobStatusEnum.Enqueued.Name);
    public static JobStatus Processing() => new JobStatus(JobStatusEnum.Processing.Name);
    public static JobStatus Completed() => new JobStatus(JobStatusEnum.Completed.Name);
    public static JobStatus Failed() => new JobStatus(JobStatusEnum.Failed.Name);
    public static JobStatus Dead() => new JobStatus(JobStatusEnum.Dead.Name);

    protected JobStatus() { } // EF Core

    private abstract class JobStatusEnum : SmartEnum<JobStatusEnum>
    {
        public static readonly JobStatusEnum Pending = new PendingType();
        public static readonly JobStatusEnum Enqueued = new EnqueuedType();
        public static readonly JobStatusEnum Processing = new ProcessingType();
        public static readonly JobStatusEnum Completed = new CompletedType();
        public static readonly JobStatusEnum Failed = new FailedType();
        public static readonly JobStatusEnum Dead = new DeadType();

        protected JobStatusEnum(string name, int value) : base(name, value)
        {
        }

        private class PendingType : JobStatusEnum
        {
            public PendingType() : base("Pending", 0)
            {
            }
        }
        
        private class EnqueuedType : JobStatusEnum
        {
            public EnqueuedType() : base("Enqueued", 1)
            {
            }
        }
        
        private class ProcessingType : JobStatusEnum
        {
            public ProcessingType() : base("Processing", 2)
            {
            }
        }

        private class CompletedType : JobStatusEnum
        {
            public CompletedType() : base("Completed", 3)
            {
            }
        }

        private class FailedType : JobStatusEnum
        {
            public FailedType() : base("Failed", 4)
            {
            }
        }
        
        private class DeadType : JobStatusEnum
        {
            public DeadType() : base("Dead", 5)
            {
            }
        }
    }
}