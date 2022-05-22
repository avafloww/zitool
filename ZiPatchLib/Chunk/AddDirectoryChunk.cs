using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk;

public class AddDirectoryChunk : ZiPatchChunk
{
    public new static string Type = "ADIR";


    public AddDirectoryChunk(ChecksumBinaryReader reader, int offset, int size) : base(reader, offset, size) { }

    public string DirName { get; protected set; }

    protected override void ReadChunk()
    {
        var start = Reader.BaseStream.Position;

        var dirNameLen = Reader.ReadUInt32BE();

        DirName = Reader.ReadFixedLengthString(dirNameLen);

        Reader.ReadBytes(Size - (int) (Reader.BaseStream.Position - start));
    }

    public override void ApplyChunk(ZiPatchConfig config)
    {
        Directory.CreateDirectory(config.GamePath + DirName);
    }

    public override string ToString()
    {
        return $"{Type}:{DirName}";
    }
}
