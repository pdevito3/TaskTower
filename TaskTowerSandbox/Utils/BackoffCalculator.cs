namespace TaskTowerSandbox.Utils;

public static class BackoffCalculator
{
    // CalculateBackoff calculates the number of seconds to back off before the next retry
    // this formula is ported from neoq who got it from Sidekiq because it is good.
    public static DateTimeOffset CalculateBackoff(int retryCount)
    {
        const int backoffExponent = 4;
        const int maxInt = 30;
        var rand = new Random();
        
        var p = Convert.ToInt32(Math.Round(Math.Pow(retryCount, backoffExponent)));
        var additionalSeconds = p + 15 + rand.Next(maxInt) * retryCount + 1;
        return DateTimeOffset.UtcNow.AddSeconds(additionalSeconds);
    }
}