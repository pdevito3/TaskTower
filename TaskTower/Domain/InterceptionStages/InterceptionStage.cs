namespace TaskTower.Domain.InterceptionStages;

using System.ComponentModel.DataAnnotations;
using Ardalis.SmartEnum;

public class InterceptionStage : ValueObject
{
    private InterceptionStageEnum _status;
    public string Value
    {
        get => _status.Name;
        private set
        {
            if (!InterceptionStageEnum.TryFromName(value, true, out var parsed))
                throw new ValidationException($"Invalid Interception Stage. Please use one of the following: {string.Join("", "", ListNames())}");

            _status = parsed;
        }
    }
    
    public InterceptionStage(string value)
    {
        Value = value;
    }

    public bool IsPreProcessing() => Value == PreProcessing().Value;
    public bool IsSuccess() => Value == Success().Value;
    public bool IsFailure() => Value == Failure().Value;
    public bool IsDeath() => Value == Death().Value;
    public static InterceptionStage Of(string value) => new InterceptionStage(value);
    public static implicit operator string(InterceptionStage value) => value.Value;
    public static List<string> ListNames() => InterceptionStageEnum.List.Select(x => x.Name).ToList();
    public static InterceptionStage PreProcessing() => new InterceptionStage(InterceptionStageEnum.PreProcessing.Name);
    public static InterceptionStage Success() => new InterceptionStage(InterceptionStageEnum.Success.Name);
    public static InterceptionStage Failure() => new InterceptionStage(InterceptionStageEnum.Failure.Name);
    public static InterceptionStage Death() => new InterceptionStage(InterceptionStageEnum.Death.Name);

    protected InterceptionStage() { } // EF Core

    private abstract class InterceptionStageEnum : SmartEnum<InterceptionStageEnum>
    {
        public static readonly InterceptionStageEnum PreProcessing = new PreProcessingType();
        public static readonly InterceptionStageEnum Success = new SuccessType();
        public static readonly InterceptionStageEnum Failure = new FailureType();
        public static readonly InterceptionStageEnum Death = new DeathType();

        protected InterceptionStageEnum(string name, int value) : base(name, value)
        {
        }

        private class PreProcessingType : InterceptionStageEnum
        {
            public PreProcessingType() : base("PreProcessing", 0)
            {
            }
        }
        
        private class SuccessType : InterceptionStageEnum
        {
            public SuccessType() : base("Success", 1)
            {
            }
        }
        
        private class FailureType : InterceptionStageEnum
        {
            public FailureType() : base("Failure", 2)
            {
            }
        }

        private class DeathType : InterceptionStageEnum
        {
            public DeathType() : base("Death", 3)
            {
            }
        }
    }
}