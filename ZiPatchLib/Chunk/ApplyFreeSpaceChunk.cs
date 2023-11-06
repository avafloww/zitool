﻿using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk
{
    public class ApplyFreeSpaceChunk : ZiPatchChunk
    {
        // This is a NOP on recent patcher versions, so I don't think we'll be seeing it.
        public new static string Type = "APFS";

        // TODO: No samples of this were found, so these fields are theoretical
        public long UnknownFieldA { get; protected set; }
        public long UnknownFieldB { get; protected set; }

        protected override void ReadChunk()
        {
            using var advanceAfter = new AdvanceOnDispose(this.Reader, Size);
            UnknownFieldA = this.Reader.ReadInt64BE();
            UnknownFieldB = this.Reader.ReadInt64BE();
        }

        public ApplyFreeSpaceChunk(ChecksumBinaryReader reader, long offset, long size) : base(reader, offset, size) {}

        public override string ToString()
        {
            return $"{Type}:{UnknownFieldA}:{UnknownFieldB}";
        }
    }
}