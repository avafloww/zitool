using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk;

public class EndOfFileChunk : ZiPatchChunk
{
    public new static string Type = "EOF_";

    public EndOfFileChunk(ChecksumBinaryReader reader, int offset, int size) : base(reader, offset, size) { }

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
