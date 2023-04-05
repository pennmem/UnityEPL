using System;
using System.Diagnostics;
using System.Threading;

// This is a high accuracy and precision timer based on a design by Nima Ara
// It is thread safe since all times are threadlocal
// It will have slight inconsistencies across threads, for the same reason
// https://nimaara.com/2016/07/06/high-resolution-clock-in-net.html
// https://stackoverflow.com/questions/1416139/how-to-get-timestamp-of-tick-precision-in-net-c
public static class Clock {
    private static readonly long _maxIdleTime = TimeSpan.FromSeconds(10).Ticks;
    private const long TicksMultiplier = 1000 * TimeSpan.TicksPerMillisecond;

    private static readonly ThreadLocal<DateTime> _startTime =
        new ThreadLocal<DateTime>(() => DateTime.UtcNow, false);

    private static readonly ThreadLocal<double> _startTimestamp =
        new ThreadLocal<double>(() => Stopwatch.GetTimestamp(), false);

    public static DateTime UtcNow {
        get {
            double endTimestamp = Stopwatch.GetTimestamp();

            var durationInTicks = (endTimestamp - _startTimestamp.Value) / Stopwatch.Frequency * TicksMultiplier;
            if (durationInTicks >= _maxIdleTime) {
                _startTimestamp.Value = Stopwatch.GetTimestamp();
                _startTime.Value = DateTime.UtcNow;
                return _startTime.Value;
            }

            return _startTime.Value.AddTicks((long)durationInTicks);
        }
    }
}
