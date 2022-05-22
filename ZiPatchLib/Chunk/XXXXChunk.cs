using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk;

// ReSharper disable once InconsistentNaming
public class XXXXChunk : ZiPatchChunk
{
    // TODO: This... Never happens.
    public new static string Type = "XXXX";

    public XXXXChunk(ChecksumBinaryReader reader, int offset, int size) : base(reader, offset, size) { }

    protected override void ReadChunk()
    {
        var start = Reader.BaseStream.Position;

        Reader.ReadBytes(Size - (int) (Reader.BaseStream.Position - start));
    }

    public override string ToString()
    {
        return Type;
    }
}
