namespace ConcurrentPipelines.Common.Extensions;

public static class StringExtension
{
    public static string ToHexColor(this string str)
    {
        if (str.Length == 0)
            return "#000";

        var hash = str
            .Aggregate(0, (current, t) => t + ((current << 5) - current));
        var color = Enumerable.Range(0, 3)
            .Aggregate("#", (current, i) => current + ("00" + ((hash >> (i * 8)) & 255).ToString("X"))[^2]);

        return color;
    }
}