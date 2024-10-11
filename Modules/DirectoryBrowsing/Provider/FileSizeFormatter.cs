using System.Globalization;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider;

public static class FileSizeFormatter
{
    private const double Kilobytes = 1024.0;

    private const double Megabytes = Kilobytes * 1024;

    private const double Gigabytes = Megabytes * 1024;

    private const double Terabytes = Gigabytes * 1024;

    public static string Format(ulong? bytes)
    {
        if (bytes is not null)
        {
            var b = (long)bytes;

            if (bytes > Terabytes)
            {
                return Math.Round(b / Terabytes, 2).ToString(CultureInfo.InvariantCulture) + " TB";
            }
            if (bytes > Gigabytes)
            {
                return Math.Round(b / Gigabytes, 2).ToString(CultureInfo.InvariantCulture) + " GB";
            }
            if (bytes > Megabytes)
            {
                return Math.Round(b / Megabytes, 2).ToString(CultureInfo.InvariantCulture) + " MB";
            }
            if (bytes > Kilobytes)
            {
                return Math.Round(b / Kilobytes, 2).ToString(CultureInfo.InvariantCulture) + " KB";
            }

            return $"{bytes} Bytes";
        }

        return "-";
    }
}
