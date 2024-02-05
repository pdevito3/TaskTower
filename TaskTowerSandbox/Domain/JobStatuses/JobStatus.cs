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
    public static JobStatus Of(string value) => new JobStatus(value);
    public static implicit operator string(JobStatus value) => value.Value;
    public static List<string> ListNames() => JobStatusEnum.List.Select(x => x.Name).ToList();
    public static JobStatus Pending() => new JobStatus(JobStatusEnum.Pending.Name);
    public static JobStatus Completed() => new JobStatus(JobStatusEnum.Completed.Name);
    public static JobStatus Failed() => new JobStatus(JobStatusEnum.Failed.Name);

    protected JobStatus() { } // EF Core

    private abstract class JobStatusEnum : SmartEnum<JobStatusEnum>
    {
        public static readonly JobStatusEnum Pending = new PendingType();
        public static readonly JobStatusEnum Completed = new CompletedType();
        public static readonly JobStatusEnum Failed = new FailedType();

        protected JobStatusEnum(string name, int value) : base(name, value)
        {
        }

        private class PendingType : JobStatusEnum
        {
            public PendingType() : base("Pending", 0)
            {
            }
        }

        private class CompletedType : JobStatusEnum
        {
            public CompletedType() : base("Completed", 1)
            {
            }
        }

        private class FailedType : JobStatusEnum
        {
            public FailedType() : base("Failed", 2)
            {
            }
        }
    }
}