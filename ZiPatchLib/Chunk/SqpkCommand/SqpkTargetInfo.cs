using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk.SqpkCommand;

public class SqpkTargetInfo : SqpkChunk
{
    // US/EU/JP are Global
    // ZH seems to also be Global
    // KR is unknown
    public enum RegionId : short
    {
        Global = -1
    }

    // Only Platform is used on recent patcher versions
    public new static string Command = "T";

    public SqpkTargetInfo(ChecksumBinaryReader reader, int offset, int size) : base(reader, offset, size) { }

    public ZiPatchConfig.PlatformId Platform { get; protected set; }
    public RegionId Region { get; protected set; }
    public bool IsDebug { get; protected set; }
    public ushort Version { get; protected set; }
    public ulong DeletedDataSize { get; protected set; }
    public ulong SeekCount { get; protected set; }

    protected override void ReadChunk()
    {
        var start = Reader.BaseStream.Position;

        // Reserved
        Reader.ReadBytes(3);

        Platform = (ZiPatchConfig.PlatformId) Reader.ReadUInt16BE();
        Region = (RegionId) Reader.ReadInt16BE();
        IsDebug = Reader.ReadInt16BE() != 0;
        Version = Reader.ReadUInt16BE();
        DeletedDataSize = Reader.ReadUInt64();
        SeekCount = Reader.ReadUInt64();

        // Empty 32 + 64 bytes
        Reader.ReadBytes(Size - (int) (Reader.BaseStream.Position - start));
    }

    public override void ApplyChunk(ZiPatchConfig config)
    {
        config.Platform = Platform;
    }

    public override string ToString()
    {
        return $"{Type}:{Command}:{Platform}:{Region}:{IsDebug}:{Version}:{DeletedDataSize}:{SeekCount}";
    }
}
