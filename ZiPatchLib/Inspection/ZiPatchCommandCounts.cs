namespace ZiPatchLib.Inspection;

public class ZiPatchCommandCounts
{
    public uint AddDirectories { get; init; }
    public uint DeleteDirectories { get; init; }
    public uint TotalCommands { get; init; }
    public uint SqpkAddCommands { get; init; }
    public uint SqpkDeleteCommands { get; init; }
    public uint SqpkExpandCommands { get; init; }
    public uint SqpkHeaderCommands { get; init; }
    public uint SqpkFileCommands { get; init; }
}
