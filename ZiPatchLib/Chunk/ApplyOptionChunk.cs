﻿using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk;

public class ApplyOptionChunk : ZiPatchChunk
{
    public enum ApplyOptionKind : uint
    {
        IgnoreMissing = 1,
        IgnoreOldMismatch = 2
    }

    public new static string Type = "APLY";

    public ApplyOptionChunk(ChecksumBinaryReader reader, int offset, int size) : base(reader, offset, size) { }

    // These are both false on all files seen
    public ApplyOptionKind OptionKind { get; protected set; }

    public bool OptionValue { get; protected set; }

    protected override void ReadChunk()
    {
        var start = Reader.BaseStream.Position;

        OptionKind = (ApplyOptionKind) Reader.ReadUInt32BE();

        // Discarded padding, always 0x0000_0004 as far as observed
        Reader.ReadBytes(4);

        var value = Reader.ReadUInt32BE() != 0;

        if (OptionKind == ApplyOptionKind.IgnoreMissing ||
            OptionKind == ApplyOptionKind.IgnoreOldMismatch)
        {
            OptionValue = value;
        }
        else
        {
            OptionValue = false; // defaults to false if OptionKind isn't valid
        }

        Reader.ReadBytes(Size - (int) (Reader.BaseStream.Position - start));
    }

    public override void ApplyChunk(ZiPatchConfig config)
    {
        switch (OptionKind)
        {
            case ApplyOptionKind.IgnoreMissing:
                config.IgnoreMissing = OptionValue;
                break;

            case ApplyOptionKind.IgnoreOldMismatch:
                config.IgnoreOldMismatch = OptionValue;
                break;
        }
    }

    public override string ToString()
    {
        return $"{Type}:{OptionKind}:{OptionValue}";
    }
}
