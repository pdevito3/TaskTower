namespace TaskTower.Processing;

using System.Linq.Expressions;

public interface IScheduleBuilder
{
    IScheduleBuilder ToQueue(string queue);
    Task<Guid> InMilliseconds(int milliseconds);
    Task<Guid> InSeconds(int seconds);
    Task<Guid> InMinutes(int minutes);
    Task<Guid> InHours(int hours);
    Task<Guid> InDays(int days);
    Task<Guid> InWeeks(int weeks);
    Task<Guid> InMonths(int months);
    Task<Guid> InYears(int years);
    Task<Guid> AtSpecificTime(DateTimeOffset time);
    Task<Guid> Immediately();
}

public class ScheduleBuilder<T> : IScheduleBuilder
{
    private readonly IBackgroundJobClient _client;
    private readonly Expression _methodCall;
    private readonly string? _queue;
    private readonly CancellationToken _cancellationToken;

    public ScheduleBuilder(IBackgroundJobClient client, Expression methodCall, string? queue, CancellationToken cancellationToken = default)
    {
        _client = client;
        _methodCall = methodCall;
        _queue = queue;
        _cancellationToken = cancellationToken;
    }
    
    public ScheduleBuilder(IBackgroundJobClient client, Expression methodCall, CancellationToken cancellationToken = default)
    {
        _client = client;
        _methodCall = methodCall;
        _queue = null;
        _cancellationToken = cancellationToken;
    }

    public IScheduleBuilder ToQueue(string queue) => new ScheduleBuilder<T>(_client, _methodCall, queue, _cancellationToken);
    public Task<Guid> InMilliseconds(int milliseconds) => Schedule(TimeSpan.FromMilliseconds(milliseconds));
    public Task<Guid> InSeconds(int seconds) => Schedule(TimeSpan.FromSeconds(seconds));
    public Task<Guid> InMinutes(int minutes) => Schedule(TimeSpan.FromMinutes(minutes));
    public Task<Guid> InHours(int hours) => Schedule(TimeSpan.FromHours(hours));
    public Task<Guid> InDays(int days) => Schedule(TimeSpan.FromDays(days));
    public Task<Guid> InWeeks(int weeks) => Schedule(TimeSpan.FromDays(7 * weeks));
    
    public Task<Guid> InMonths(int months)
        => AtSpecificTime(DateTimeOffset.UtcNow.AddMonths(months));
    
    public Task<Guid> InYears(int years)
        => AtSpecificTime(DateTimeOffset.UtcNow.AddYears(years));
    
    public Task<Guid> Immediately() => Schedule(TimeSpan.Zero);
    public Task<Guid> AtSpecificTime(DateTimeOffset dateTime)
    {
        var now = DateTimeOffset.UtcNow;
        var delay = dateTime > now ? dateTime - now : TimeSpan.Zero;
        return Schedule(delay);
    }
    
    private Task<Guid> Schedule(TimeSpan delay)
    {
        switch (_methodCall)
        {
            case Expression<Action> action:
                return _client.Schedule(action, delay, _queue, _cancellationToken);
            case Expression<Action<T>> actionT:
                return _client.Schedule(actionT, delay, _queue, _cancellationToken);
            case Expression<Func<T, Task>> funcT:
                return _client.Schedule(funcT, delay, _queue, _cancellationToken);
            default:
                throw new InvalidOperationException("Unsupported expression type.");
        }
    }
}
