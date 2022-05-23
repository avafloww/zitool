using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk.SqpkCommand;

public class SqpkPatchInfo : SqpkChunk
{
    // This is a NOP on recent patcher versions
    public new static string Command = "X";

    public SqpkPatchInfo(ChecksumBinaryReader reader, int offset, int size) : base(reader, offset, size) { }

    // Don't know what this stuff is for
    public byte Status { get; protected set; }
    public byte Version { get; protected set; }
    public ulong InstallSize { get; protected set; }

    protected override void ReadChunk()
    {
        var start = Reader.BaseStream.Position;

        Status = Reader.ReadByte();
        Version = Reader.ReadByte();
        Reader.ReadByte(); // Alignment

        InstallSize = Reader.ReadUInt64BE();

        Reader.ReadBytes(Size - (int) (Reader.BaseStream.Position - start));
    }

    public override string ToString()
    {
        return $"{Type}:{Command}:{Status}:{Version}:{InstallSize}";
    }
}
