namespace ApplyUpdateInfo;

public static class TimeTokenService
{
    public static readonly TimeSpan JapanStandardTimeOffset = TimeSpan.FromHours(9);

    public static DateTime GetJapanStandardTime()
    {
        return DateTimeOffset.UtcNow.ToOffset(JapanStandardTimeOffset).DateTime;
    }
}
