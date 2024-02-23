namespace TaskTower;

public class TaskTowerConstants
{
    public const string ConnectionString =
        "Host=localhost;Port=41444;Database=dev_hello_task_tower_sandbox;Username=postgres;Password=postgres;Pooling=true;MinPoolSize=1;MaxPoolSize=100;";
    
    public static class Notifications
    {
        public const string JobAvailable = "job_available";
    }
    public static class Configuration
    {
        public const int MinimumWaitIntervalMilliseconds = 500;
    }
}