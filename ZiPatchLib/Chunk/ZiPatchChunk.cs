using System.Reflection;
using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk;

public abstract class ZiPatchChunk
{
    private static readonly AsyncLocal<MemoryStream> localMemoryStream = new();


    // Only FileHeader, ApplyOption, Sqpk, and EOF have been observed in XIVARR+ patches
    // AddDirectory and DeleteDirectory can theoretically happen, so they're implemented
    // ApplyFreeSpace doesn't seem to show up anymore, and EntryFile will just error out
    private static readonly Dictionary<string, Func<ChecksumBinaryReader, int, int, ZiPatchChunk>> ChunkTypes =
        new()
        {
            {FileHeaderChunk.Type, (reader, offset, size) => new FileHeaderChunk(reader, offset, size)},
            {ApplyOptionChunk.Type, (reader, offset, size) => new ApplyOptionChunk(reader, offset, size)},
            {ApplyFreeSpaceChunk.Type, (reader, offset, size) => new ApplyFreeSpaceChunk(reader, offset, size)},
            {AddDirectoryChunk.Type, (reader, offset, size) => new AddDirectoryChunk(reader, offset, size)},
            {DeleteDirectoryChunk.Type, (reader, offset, size) => new DeleteDirectoryChunk(reader, offset, size)},
            {SqpkChunk.Type, SqpkChunk.GetCommand},
            {EndOfFileChunk.Type, (reader, offset, size) => new EndOfFileChunk(reader, offset, size)},
            {XXXXChunk.Type, (reader, offset, size) => new XXXXChunk(reader, offset, size)}
        };

    protected readonly ChecksumBinaryReader Reader;

    protected ZiPatchChunk(ChecksumBinaryReader reader, int offset, int size)
    {
        Reader = reader;

        Offset = offset;
        Size = size;
    }

    public static string Type { get; protected set; }

    // Hack: C# doesn't let you get static fields from instances.
    public virtual string ChunkType => (string) GetType()
        .GetField("Type", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public)
        ?.GetValue(null);

    public int Offset { get; protected set; }
    public int Size { get; protected set; }
    public uint Checksum { get; protected set; }
    public uint CalculatedChecksum { get; protected set; }

    public bool IsChecksumValid => CalculatedChecksum == Checksum;


    public static ZiPatchChunk GetChunk(Stream stream)
    {
        localMemoryStream.Value = localMemoryStream.Value ?? new MemoryStream();

        var memoryStream = localMemoryStream.Value;
        try
        {
            var reader = new BinaryReader(stream);
            var size = reader.ReadInt32BE();
            var baseOffset = (int) stream.Position;

            // size of chunk + header + checksum
            var readSize = size + 4 + 4;

            // Enlarge MemoryStream if necessary, or set length at capacity
            var maxLen = Math.Max(readSize, memoryStream.Capacity);
            if (memoryStream.Length < maxLen)
            {
                memoryStream.SetLength(maxLen);
            }

            // Read into MemoryStream's inner buffer
            reader.BaseStream.Read(memoryStream.GetBuffer(), 0, readSize);

            var binaryReader = new ChecksumBinaryReader(memoryStream);
            binaryReader.InitCrc32();

            var type = binaryReader.ReadFixedLengthString(4u);
            if (!ChunkTypes.TryGetValue(type, out var constructor))
            {
                throw new ZiPatchException();
            }

            var chunk = constructor(binaryReader, baseOffset, size);

            chunk.ReadChunk();
            chunk.ReadChecksum();
            return chunk;
        }
        catch (EndOfStreamException e)
        {
            throw new ZiPatchException("Could not get chunk", e);
        }
        finally
        {
            memoryStream.Position = 0;
        }
    }

    protected virtual void ReadChunk()
    {
        Reader.ReadBytes(Size);
    }

    public virtual void ApplyChunk(ZiPatchConfig config) { }

    protected void ReadChecksum()
    {
        CalculatedChecksum = Reader.GetCrc32();
        Checksum = Reader.ReadUInt32BE();
    }
}
