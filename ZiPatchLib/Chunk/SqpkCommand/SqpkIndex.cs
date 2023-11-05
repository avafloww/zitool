﻿using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk.SqpkCommand
{
    class SqpkIndex : SqpkChunk
    {
        // This is a NOP on recent patcher versions.
        public new static string Command = "I";

        public enum IndexCommandKind : byte
        {
            Add = (byte)'A',
            Delete = (byte)'D'
        }

        public IndexCommandKind IndexCommand { get; protected set; }
        public bool IsSynonym { get; protected set; }
        public SqpackIndexFile TargetFile { get; protected set; }
        public ulong FileHash { get; protected set; }
        public uint BlockOffset { get; protected set; }

        // TODO: Figure out what this is used for
        public uint BlockNumber { get; protected set; }



        public SqpkIndex(ChecksumBinaryReader reader, long offset, long size) : base(reader, offset, size) {}

        protected override void ReadChunk()
        {
            using var advanceAfter = new AdvanceOnDispose(this.Reader, Size);
            IndexCommand = (IndexCommandKind)this.Reader.ReadByte();
            IsSynonym = this.Reader.ReadBoolean();
            this.Reader.ReadByte(); // Alignment

            TargetFile = new SqpackIndexFile(this.Reader);

            FileHash = this.Reader.ReadUInt64BE();

            BlockOffset = this.Reader.ReadUInt32BE();
            BlockNumber = this.Reader.ReadUInt32BE();
        }

        public override string ToString()
        {
            return $"{Type}:{Command}:{IndexCommand}:{IsSynonym}:{TargetFile}:{FileHash:X8}:{BlockOffset}:{BlockNumber}";
        }
    }
}