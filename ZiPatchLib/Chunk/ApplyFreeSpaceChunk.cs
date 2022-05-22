using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk;

public class ApplyFreeSpaceChunk : ZiPatchChunk
{
    // This is a NOP on recent patcher versions, so I don't think we'll be seeing it.
    public new static string Type = "APFS";

    public ApplyFreeSpaceChunk(ChecksumBinaryReader reader, int offset, int size) : base(reader, offset, size) { }

    // TODO: No samples of this were found, so these fields are theoretical
    public long UnknownFieldA { get; protected set; }
    public long UnknownFieldB { get; protected set; }

    protected override void ReadChunk()
    {
        var start = Reader.BaseStream.Position;

        UnknownFieldA = Reader.ReadInt64BE();
        UnknownFieldB = Reader.ReadInt64BE();

        Reader.ReadBytes(Size - (int) (Reader.BaseStream.Position - start));
    }

    public override string ToString()
    {
        return $"{Type}:{UnknownFieldA}:{UnknownFieldB}";
    }
}
