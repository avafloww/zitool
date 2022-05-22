namespace ZiPatchLib.Util;

public class ChecksumBinaryReader : BinaryReader
{
    private readonly Crc32 _crc32 = new();

    public ChecksumBinaryReader(Stream input) : base(input) { }


    public void InitCrc32()
    {
        _crc32.Init();
    }

    public uint GetCrc32()
    {
        return _crc32.Checksum;
    }

    public override byte[] ReadBytes(int count)
    {
        var result = base.ReadBytes(count);

        _crc32.Update(result);

        return result;
    }

    public override byte ReadByte()
    {
        var result = base.ReadByte();

        _crc32.Update(result);

        return result;
    }

    public override sbyte ReadSByte()
    {
        return (sbyte) ReadByte();
    }

    public override bool ReadBoolean()
    {
        return ReadByte() != 0;
    }

    public override char ReadChar()
    {
        return (char) ReadByte();
    }

    public override short ReadInt16()
    {
        return BitConverter.ToInt16(ReadBytes(sizeof(short)), 0);
    }

    public override ushort ReadUInt16()
    {
        return BitConverter.ToUInt16(ReadBytes(sizeof(ushort)), 0);
    }

    public override int ReadInt32()
    {
        return BitConverter.ToInt32(ReadBytes(sizeof(int)), 0);
    }

    public override uint ReadUInt32()
    {
        return BitConverter.ToUInt32(ReadBytes(sizeof(uint)), 0);
    }

    public override long ReadInt64()
    {
        return BitConverter.ToInt64(ReadBytes(sizeof(long)), 0);
    }

    public override ulong ReadUInt64()
    {
        return BitConverter.ToUInt64(ReadBytes(sizeof(ulong)), 0);
    }

    public override float ReadSingle()
    {
        return BitConverter.ToSingle(ReadBytes(sizeof(float)), 0);
    }

    public override double ReadDouble()
    {
        return BitConverter.ToDouble(ReadBytes(sizeof(float)), 0);
    }
}
