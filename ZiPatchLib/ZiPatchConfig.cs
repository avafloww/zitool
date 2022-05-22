using ZiPatchLib.Util;

namespace ZiPatchLib;

public class ZiPatchConfig
{
    public enum PlatformId : ushort
    {
        Win32 = 0,
        Ps3 = 1,
        Ps4 = 2,
        Unknown = 3
    }


    public ZiPatchConfig(string gamePath)
    {
        GamePath = gamePath;
    }

    public string GamePath { get; protected set; }
    public PlatformId Platform { get; set; }
    public bool IgnoreMissing { get; set; }
    public bool IgnoreOldMismatch { get; set; }
    public SqexFileStreamStore Store { get; set; }
}
