namespace TaskTower.Domain.RunHistoryStatuses;

using System.ComponentModel.DataAnnotations;
using Ardalis.SmartEnum;

public class RunHistoryStatus : ValueObject
{
    private RunHistoryStatusEnum _status;
    public string Value
    {
        get => _status.Name;
        private set
        {
            if (!RunHistoryStatusEnum.TryFromName(value, true, out var parsed))
                throw new ValidationException($"Invalid Status. Please use one of the following: {string.Join("", "", ListNames())}");

            _status = parsed;
        }
    }
    
    public RunHistoryStatus(string value)
    {
        Value = value;
    }

    public bool IsPending() => Value == Pending().Value;
    public bool IsEnqueued() => Value == Enqueued().Value;
    public bool IsProcessing() => Value == Processing().Value;
    public bool IsCompleted() => Value == Completed().Value;
    public bool IsFailed() => Value == Failed().Value;
    public bool IsDead() => Value == Dead().Value;
    public bool IsRequeued() => Value == Requeued().Value;
    public static RunHistoryStatus Of(string value) => new RunHistoryStatus(value);
    public static implicit operator string(RunHistoryStatus value) => value.Value;
    public static List<string> ListNames() => RunHistoryStatusEnum.List.Select(x => x.Name).ToList();
    public static RunHistoryStatus Pending() => new RunHistoryStatus(RunHistoryStatusEnum.Pending.Name);
    public static RunHistoryStatus Enqueued() => new RunHistoryStatus(RunHistoryStatusEnum.Enqueued.Name);
    public static RunHistoryStatus Processing() => new RunHistoryStatus(RunHistoryStatusEnum.Processing.Name);
    public static RunHistoryStatus Completed() => new RunHistoryStatus(RunHistoryStatusEnum.Completed.Name);
    public static RunHistoryStatus Failed() => new RunHistoryStatus(RunHistoryStatusEnum.Failed.Name);
    public static RunHistoryStatus Dead() => new RunHistoryStatus(RunHistoryStatusEnum.Dead.Name);
    public static RunHistoryStatus Requeued() => new RunHistoryStatus(RunHistoryStatusEnum.Requeued.Name);

    protected RunHistoryStatus() { } // EF Core

    private abstract class RunHistoryStatusEnum : SmartEnum<RunHistoryStatusEnum>
    {
        public static readonly RunHistoryStatusEnum Pending = new PendingType();
        public static readonly RunHistoryStatusEnum Enqueued = new EnqueuedType();
        public static readonly RunHistoryStatusEnum Processing = new ProcessingType();
        public static readonly RunHistoryStatusEnum Completed = new CompletedType();
        public static readonly RunHistoryStatusEnum Failed = new FailedType();
        public static readonly RunHistoryStatusEnum Dead = new DeadType();
        public static readonly RunHistoryStatusEnum Requeued = new RequeuedType();

        protected RunHistoryStatusEnum(string name, int value) : base(name, value)
        {
        }

        private class PendingType : RunHistoryStatusEnum
        {
            public PendingType() : base("Pending", 0)
            {
            }
        }
        
        private class EnqueuedType : RunHistoryStatusEnum
        {
            public EnqueuedType() : base("Enqueued", 1)
            {
            }
        }
        
        private class ProcessingType : RunHistoryStatusEnum
        {
            public ProcessingType() : base("Processing", 2)
            {
            }
        }

        private class CompletedType : RunHistoryStatusEnum
        {
            public CompletedType() : base("Completed", 3)
            {
            }
        }

        private class FailedType : RunHistoryStatusEnum
        {
            public FailedType() : base("Failed", 4)
            {
            }
        }
        
        private class DeadType : RunHistoryStatusEnum
        {
            public DeadType() : base("Dead", 5)
            {
            }
        }
        
        private class RequeuedType : RunHistoryStatusEnum
        {
            public RequeuedType() : base("Requeued", 6)
            {
            }
        }
    }
}