using System;
using System.Diagnostics;
using System.Threading;

public static class HighResolutionDateTime 
{
    private static long _startTime = DateTime.UtcNow.Ticks;
    private static long _startTimestamp = Stopwatch.GetTimestamp();

    public static DateTime UtcNow
    {
        get
        {
            long endTimestamp = Stopwatch.GetTimestamp();
            var tickDuration = (endTimestamp - _startTimestamp);
            return new DateTime(_startTime + tickDuration);
        }
    }

    public static void Set() {
        Interlocked.Exchange(ref _startTime, DateTime.UtcNow.Ticks);
        Interlocked.Exchange(ref _startTimestamp, Stopwatch.GetTimestamp());
    }
}