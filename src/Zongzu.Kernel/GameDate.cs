using System;

namespace Zongzu.Kernel;

public readonly record struct GameDate
{
    public GameDate(int year, int month)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be between 1 and 12.");
        }

        Year = year;
        Month = month;
    }

    public int Year { get; }

    public int Month { get; }

    public GameDate NextMonth()
    {
        return Month == 12 ? new GameDate(Year + 1, 1) : new GameDate(Year, Month + 1);
    }

    public override string ToString()
    {
        return $"{Year:D4}-{Month:D2}";
    }
}
