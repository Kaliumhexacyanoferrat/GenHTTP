using System;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider
{

    public static class FileSizeFormatter
    {

        private const double KILOBYTES = 1024.0;

        private const double MEGABYTES = KILOBYTES * 1024;

        private const double GIGABYTES = MEGABYTES * 1024;

        private const double TERABYTES = GIGABYTES * 1024;

        public static string Format(ulong? bytes)
        {
            if (bytes is not null)
            {
                var b = (long)bytes;

                if (bytes > TERABYTES)
                {
                    return Math.Round(b / TERABYTES, 2) + " TB";
                }
                if (bytes > GIGABYTES)
                {
                    return Math.Round(b / GIGABYTES, 2) + " GB";
                }
                else if (bytes > MEGABYTES)
                {
                    return Math.Round(b / MEGABYTES, 2) + " MB";
                }
                else if (bytes > KILOBYTES)
                {
                    return Math.Round(b / KILOBYTES, 2) + " KB";
                }

                return $"{bytes} Bytes";
            }

            return "-";
        }

    }

}
