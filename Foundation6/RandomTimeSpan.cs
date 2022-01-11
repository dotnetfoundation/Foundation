﻿namespace Foundation;

/// <summary>
/// A random TimeSpan generator.
/// </summary>
public struct RandomTimeSpan
{
    private readonly Random _random;

    public RandomTimeSpan(TimeSpan min, TimeSpan max) : this(new Random(), min, max)
    {
    }

    public RandomTimeSpan(TimeSpan min, TimeSpan max, int seed) : this(new Random(seed), min, max)
    {
    }

    private RandomTimeSpan(Random random, TimeSpan min, TimeSpan max)
    {
        _random = random.ThrowIfNull(nameof(random));
        if (min > max) throw new ArgumentOutOfRangeException("max must be greater than min");

        Min = min;
        Max = max;
    }

    public bool IsEmpty => null == _random;

    public TimeSpan Max { get; }

    public TimeSpan Min { get; }

    public TimeSpan Next()
    {
        var delayInMilliseconds = _random.NextDouble(Min.TotalMilliseconds, Max.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(delayInMilliseconds);
    }
}

