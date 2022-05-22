using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk.SqpkCommand;

internal class SqpkIndex : SqpkChunk
{
    public enum IndexCommandKind : byte
    {
        Add = (byte) 'A',
        Delete = (byte) 'D'
    }

    // This is a NOP on recent patcher versions.
    public new static string Command = "I";


    public SqpkIndex(ChecksumBinaryReader reader, int offset, int size) : base(reader, offset, size) { }

    public IndexCommandKind IndexCommand { get; protected set; }
    public bool IsSynonym { get; protected set; }
    public SqpackIndexFile TargetFile { get; protected set; }
    public ulong FileHash { get; protected set; }
    public uint BlockOffset { get; protected set; }

    // TODO: Figure out what this is used for
    public uint BlockNumber { get; protected set; }

    protected override void ReadChunk()
    {
        var start = Reader.BaseStream.Position;

        IndexCommand = (IndexCommandKind) Reader.ReadByte();
        IsSynonym = Reader.ReadBoolean();
        Reader.ReadByte(); // Alignment

        TargetFile = new SqpackIndexFile(Reader);

        FileHash = Reader.ReadUInt64BE();

        BlockOffset = Reader.ReadUInt32BE();
        BlockNumber = Reader.ReadUInt32BE();

        Reader.ReadBytes(Size - (int) (Reader.BaseStream.Position - start));
    }

    public override string ToString()
    {
        return
            $"{Type}:{Command}:{IndexCommand}:{IsSynonym}:{TargetFile}:{FileHash:X8}:{BlockOffset}:{BlockNumber}";
    }
}
