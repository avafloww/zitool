using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk.SqpkCommand;

public class SqpkHeader : SqpkChunk
{
    public enum TargetFileKind : byte
    {
        Dat = (byte) 'D',
        Index = (byte) 'I'
    }

    public enum TargetHeaderKind : byte
    {
        Version = (byte) 'V',
        Index = (byte) 'I',
        Data = (byte) 'D'
    }

    public const int HEADER_SIZE = 1024;
    public new static string Command = "H";

    public SqpkHeader(ChecksumBinaryReader reader, int offset, int size) : base(reader, offset, size) { }

    public TargetFileKind FileKind { get; protected set; }
    public TargetHeaderKind HeaderKind { get; protected set; }
    public SqpackFile TargetFile { get; protected set; }

    public byte[] HeaderData { get; protected set; }
    public long HeaderDataSourceOffset { get; protected set; }

    protected override void ReadChunk()
    {
        var start = Reader.BaseStream.Position;

        FileKind = (TargetFileKind) Reader.ReadByte();
        HeaderKind = (TargetHeaderKind) Reader.ReadByte();
        Reader.ReadByte(); // Alignment

        if (FileKind == TargetFileKind.Dat)
        {
            TargetFile = new SqpackDatFile(Reader);
        }
        else
        {
            TargetFile = new SqpackIndexFile(Reader);
        }

        HeaderDataSourceOffset = Offset + Reader.BaseStream.Position;
        HeaderData = Reader.ReadBytes(HEADER_SIZE);

        Reader.ReadBytes(Size - (int) (Reader.BaseStream.Position - start));
    }

    public override void ApplyChunk(ZiPatchConfig config)
    {
        TargetFile.ResolvePath(config.Platform);

        var file = config.Store == null
            ? TargetFile.OpenStream(config.GamePath, FileMode.OpenOrCreate)
            : TargetFile.OpenStream(config.Store, config.GamePath, FileMode.OpenOrCreate);

        file.WriteFromOffset(HeaderData, HeaderKind == TargetHeaderKind.Version ? 0 : HEADER_SIZE);
    }

    public override string ToString()
    {
        return $"{Type}:{Command}:{FileKind}:{HeaderKind}:{TargetFile}";
    }
}
