using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Scheduler.Domain.Services;

public static class ColorService
{
    private static Random random = new Random();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Not used for security")]
    public static string GenerateRandomHexColor()
    {
        int r = random.Next(256);
        int g = random.Next(256);
        int b = random.Next(256);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    public static bool IsValidHexColor(string hexColor)
    {
        if (string.IsNullOrEmpty(hexColor))
            return false;

        string pattern = "^#([A-Fa-f0-9]{6})$";

        return Regex.IsMatch(hexColor, pattern);
    }
}
