﻿using System.IO;
using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk.SqpkCommand
{
    public class SqpkDeleteData : SqpkChunk
    {
        public new static string Command = "D";


        public SqpackDatFile TargetFile { get; protected set; }
        public long BlockOffset { get; protected set; }
        public long BlockNumber { get; protected set; }


        public SqpkDeleteData(ChecksumBinaryReader reader, long offset, long size) : base(reader, offset, size) {}

        protected override void ReadChunk()
        {
            using var advanceAfter = new AdvanceOnDispose(this.Reader, Size);
            this.Reader.ReadBytes(3); // Alignment

            TargetFile = new SqpackDatFile(this.Reader);

            BlockOffset = (long)this.Reader.ReadUInt32BE() << 7;
            BlockNumber = this.Reader.ReadUInt32BE();

            this.Reader.ReadUInt32(); // Reserved
        }

        public override void ApplyChunk(ZiPatchConfig config)
        {
            TargetFile.ResolvePath(config.Platform);

            var file = config.Store == null ?
                TargetFile.OpenStream(config.GamePath, FileMode.OpenOrCreate) :
                TargetFile.OpenStream(config.Store, config.GamePath, FileMode.OpenOrCreate);

            SqpackDatFile.WriteEmptyFileBlockAt(file, BlockOffset, BlockNumber);
        }

        public override string ToString()
        {
            return $"{Type}:{Command}:{TargetFile}:{BlockOffset}:{BlockNumber}";
        }
    }
}