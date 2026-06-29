using System;
using System.Globalization;

public static class CompactNumberFormatter
{
    private static readonly string[] Units =
    {
        "",
        "A",
        "B",
        "C",
        "D",
        "E",
        "F",
        "G",
        "H",
        "I",
        "J",
        "K",
        "L",
        "M",
        "N",
        "O",
        "P",
        "Q",
        "R",
        "S",
        "T",
        "U",
        "V",
        "W",
        "X",
        "Y",
        "Z"
    };

    public static string Format(long value, string prefix = "")
    {
        double amount = value < 0L ? -(double)value : value;
        int unitIndex = 0;
        while (amount >= 1000d && unitIndex < Units.Length - 1)
        {
            amount /= 1000d;
            unitIndex++;
        }

        double display = unitIndex == 0
            ? Math.Floor(amount)
            : amount >= 100d
                ? Math.Floor(amount)
                : amount >= 10d
                    ? Math.Floor(amount * 10d) / 10d
                    : Math.Floor(amount * 100d) / 100d;

        string number = unitIndex == 0
            ? display.ToString("0", CultureInfo.InvariantCulture)
            : display >= 100d
                ? display.ToString("0", CultureInfo.InvariantCulture)
                : display >= 10d
                    ? display.ToString("0.#", CultureInfo.InvariantCulture)
                    : display.ToString("0.##", CultureInfo.InvariantCulture);

        string sign = value < 0L ? "-" : prefix;
        return sign + number + Units[unitIndex];
    }
}
