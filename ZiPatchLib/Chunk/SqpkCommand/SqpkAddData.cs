﻿using System.IO;
using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk.SqpkCommand
{
    public class SqpkAddData : SqpkChunk
    {
        public new static string Command = "A";
        
        public SqpackDatFile TargetFile { get; protected set; }
        public long BlockOffset { get; protected set; }
        public long BlockNumber { get; protected set; }
        public long BlockDeleteNumber { get; protected set; }

        public byte[] BlockData { get; protected set; }
        public long BlockDataSourceOffset { get; protected set; }


        public SqpkAddData(ChecksumBinaryReader reader, long offset, long size) : base(reader, offset, size) {}

        protected override void ReadChunk()
        {
            using var advanceAfter = new AdvanceOnDispose(this.Reader, Size);
            this.Reader.ReadBytes(3); // Alignment

            TargetFile = new SqpackDatFile(this.Reader);

            BlockOffset = (long)this.Reader.ReadUInt32BE() << 7;
            BlockNumber = (long)this.Reader.ReadUInt32BE() << 7;
            BlockDeleteNumber = (long)this.Reader.ReadUInt32BE() << 7;

            BlockDataSourceOffset = Offset + this.Reader.BaseStream.Position;
            BlockData = this.Reader.ReadBytes(checked((int)BlockNumber));
        }

        public override void ApplyChunk(ZiPatchConfig config)
        {
            TargetFile.ResolvePath(config.Platform);

            var file = config.Store == null ?
                TargetFile.OpenStream(config.GamePath, FileMode.OpenOrCreate) :
                TargetFile.OpenStream(config.Store, config.GamePath, FileMode.OpenOrCreate);

            file.WriteFromOffset(BlockData, BlockOffset);
            file.Wipe(BlockDeleteNumber);
        }

        public override string ToString()
        {
            return $"{Type}:{Command}:{TargetFile}:{BlockOffset}:{BlockNumber}:{BlockDeleteNumber}";
        }
    }
}