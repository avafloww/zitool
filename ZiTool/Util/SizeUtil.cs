namespace ZiTool.Util;

public static class SizeUtil
{
    public static string GetHumanSize(this FileInfo fi)
    {
        return GetBytesReadable(fi.Length);
    }

    public static string GetBytesReadable(long i)
    {
        // Get absolute value
        var abs = (i < 0 ? -i : i);
        // Determine the suffix and readable value
        string suffix;
        double readable;
        switch (abs)
        {
            // Exabyte
            case >= 0x1000000000000000:
                suffix = "EB";
                readable = (i >> 50);
                break;
            // Petabyte
            case >= 0x4000000000000:
                suffix = "PB";
                readable = (i >> 40);
                break;
            // Terabyte
            case >= 0x10000000000:
                suffix = "TB";
                readable = (i >> 30);
                break;
            // Gigabyte
            case >= 0x40000000:
                suffix = "GB";
                readable = (i >> 20);
                break;
            // Megabyte
            case >= 0x100000:
                suffix = "MB";
                readable = (i >> 10);
                break;
            // Kilobyte
            case >= 0x400:
                suffix = "KB";
                readable = i;
                break;
            default:
                return i.ToString("0 B"); // Byte
        }

        // Divide by 1024 to get fractional value
        readable = (readable / 1024);
        // Return formatted number with suffix
        return readable.ToString("0.### ") + suffix;
    }
}
