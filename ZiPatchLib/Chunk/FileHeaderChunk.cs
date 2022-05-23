using ZiPatchLib.Inspection;
using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk;

public class FileHeaderChunk : ZiPatchChunk
{
    public new static string Type = "FHDR";

    public FileHeaderChunk(ChecksumBinaryReader reader, int offset, int size) : base(reader, offset, size) { }

    // V1?/2
    public byte Version { get; protected set; }
    public string PatchType { get; protected set; }
    public uint EntryFiles { get; protected set; }

    public ZiPatchCommandCounts? CommandCounts { get; protected set; }

    // V3
    public long DeleteDataSize { get; protected set; } // Split in 2 DWORD; Low, High
    public uint MinorVersion { get; protected set; }
    public uint RepositoryName { get; protected set; }

    protected override void ReadChunk()
    {
        var start = Reader.BaseStream.Position;

        Version = (byte) (Reader.ReadUInt32() >> 16);
        PatchType = Reader.ReadFixedLengthString(4u);
        EntryFiles = Reader.ReadUInt32BE();

        if (Version == 3)
        {
            uint adir = Reader.ReadUInt32BE();
            uint deld = Reader.ReadUInt32BE();
            DeleteDataSize = Reader.ReadUInt32BE() | ((long) Reader.ReadUInt32BE() << 32);
            MinorVersion = Reader.ReadUInt32BE();
            RepositoryName = Reader.ReadUInt32BE();
            CommandCounts = new ZiPatchCommandCounts
            {
                AddDirectories = adir,
                DeleteDirectories = deld,
                TotalCommands = Reader.ReadUInt32BE(),
                SqpkAddCommands = Reader.ReadUInt32BE(),
                SqpkDeleteCommands = Reader.ReadUInt32BE(),
                SqpkExpandCommands = Reader.ReadUInt32BE(),
                SqpkHeaderCommands = Reader.ReadUInt32BE(),
                SqpkFileCommands = Reader.ReadUInt32BE()
            };
        }

        // 0xB8 of unknown data for V3, 0x08 of 0x00 for V2
        // ... Probably irrelevant.
        Reader.ReadBytes(Size - (int) (Reader.BaseStream.Position - start));
    }

    public override string ToString()
    {
        return $"{Type}:V{Version}:{RepositoryName}";
    }
}
