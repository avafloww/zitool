﻿using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk.SqpkCommand;

public class SqpkExpandData : SqpkChunk
{
    public new static string Command = "E";


    public SqpkExpandData(ChecksumBinaryReader reader, int offset, int size) : base(reader, offset, size) { }


    public SqpackDatFile TargetFile { get; protected set; }
    public int BlockOffset { get; protected set; }
    public int BlockNumber { get; protected set; }

    protected override void ReadChunk()
    {
        var start = Reader.BaseStream.Position;

        Reader.ReadBytes(3);

        TargetFile = new SqpackDatFile(Reader);

        BlockOffset = Reader.ReadInt32BE() << 7;
        BlockNumber = Reader.ReadInt32BE();

        Reader.ReadUInt32(); // Reserved

        Reader.ReadBytes(Size - (int) (Reader.BaseStream.Position - start));
    }

    public override void ApplyChunk(ZiPatchConfig config)
    {
        TargetFile.ResolvePath(config.Platform);

        var file = config.Store == null
            ? TargetFile.OpenStream(config.GamePath, FileMode.OpenOrCreate)
            : TargetFile.OpenStream(config.Store, config.GamePath, FileMode.OpenOrCreate);

        SqpackDatFile.WriteEmptyFileBlockAt(file, BlockOffset, BlockNumber);
    }

    public override string ToString()
    {
        return $"{Type}:{Command}:{BlockOffset}:{BlockNumber}";
    }
}
