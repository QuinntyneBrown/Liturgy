using System.Globalization;

namespace Liturgy.Application.Board;

/// <summary>Derives project card codes (e.g. "Lantern" → "LAN-34").</summary>
public static class CardCode
{
    public static string Prefix(string projectName)
    {
        var letters = new string(projectName.Where(char.IsLetter).ToArray());
        var prefix = letters.Length >= 3 ? letters[..3] : letters;
        return prefix.ToUpperInvariant();
    }

    /// <summary>Next code after the highest numeric suffix already used, so it never collides.</summary>
    public static string Next(string prefix, IEnumerable<string> existingCodes)
    {
        var max = 0;
        foreach (var code in existingCodes)
        {
            var dash = code.LastIndexOf('-');
            if (dash >= 0
                && int.TryParse(code[(dash + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)
                && n > max)
            {
                max = n;
            }
        }

        return $"{prefix}-{max + 1:00}";
    }
}
