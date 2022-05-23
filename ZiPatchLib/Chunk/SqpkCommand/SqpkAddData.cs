using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk.SqpkCommand;

public class SqpkAddData : SqpkChunk
{
    public new static string Command = "A";


    public SqpkAddData(ChecksumBinaryReader reader, int offset, int size) : base(reader, offset, size) { }


    public SqpackDatFile TargetFile { get; protected set; }
    public int BlockOffset { get; protected set; }
    public int BlockNumber { get; protected set; }
    public int BlockDeleteNumber { get; protected set; }

    public byte[] BlockData { get; protected set; }
    public long BlockDataSourceOffset { get; protected set; }

    protected override void ReadChunk()
    {
        var start = Reader.BaseStream.Position;

        Reader.ReadBytes(3); // Alignment

        TargetFile = new SqpackDatFile(Reader);

        BlockOffset = Reader.ReadInt32BE() << 7;
        BlockNumber = Reader.ReadInt32BE() << 7;
        BlockDeleteNumber = Reader.ReadInt32BE() << 7;

        BlockDataSourceOffset = Offset + Reader.BaseStream.Position;
        BlockData = Reader.ReadBytes(BlockNumber);

        Reader.ReadBytes(Size - (int) (Reader.BaseStream.Position - start));
    }

    public override void ApplyChunk(ZiPatchConfig config)
    {
        TargetFile.ResolvePath(config.Platform);

        var file = config.Store == null
            ? TargetFile.OpenStream(config.GamePath, FileMode.OpenOrCreate)
            : TargetFile.OpenStream(config.Store, config.GamePath, FileMode.OpenOrCreate);

        file.WriteFromOffset(BlockData, BlockOffset);
        file.Wipe(BlockDeleteNumber);
    }

    public override string ToString()
    {
        return $"{Type}:{Command}:{TargetFile}:{BlockOffset}:{BlockNumber}:{BlockDeleteNumber}";
    }
}
