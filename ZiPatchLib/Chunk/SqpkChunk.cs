﻿using System;
using System.Collections.Generic;
using System.IO;
using ZiPatchLib.Chunk.SqpkCommand;
using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk
{
    public abstract class SqpkChunk : ZiPatchChunk
    {
        public new static string Type = "SQPK";
        public static string Command { get; protected set; }


        private static readonly Dictionary<string, Func<ChecksumBinaryReader, long, long, SqpkChunk>> CommandTypes =
            new()
            {
                { SqpkAddData.Command, (reader, offset, size) => new SqpkAddData(reader, offset, size) },
                { SqpkDeleteData.Command, (reader, offset, size) => new SqpkDeleteData(reader, offset, size) },
                { SqpkHeader.Command, (reader, offset, size) => new SqpkHeader(reader, offset, size) },
                { SqpkTargetInfo.Command, (reader, offset, size) => new SqpkTargetInfo(reader, offset, size) },
                { SqpkExpandData.Command, (reader, offset, size) => new SqpkExpandData(reader, offset, size) },
                { SqpkIndex.Command, (reader, offset, size) => new SqpkIndex(reader, offset, size) },
                { SqpkFile.Command, (reader, offset, size) => new SqpkFile(reader, offset, size) },
                { SqpkPatchInfo.Command, (reader, offset, size) => new SqpkPatchInfo(reader, offset, size) }
            };

        public static ZiPatchChunk GetCommand(ChecksumBinaryReader reader, long offset, long size)
        {
            try
            {
                // Have not seen this differ from size
                var innerSize = reader.ReadInt32BE();
                if (size != innerSize)
                    throw new ZiPatchException();

                var command = reader.ReadFixedLengthString(1u);
                if (!CommandTypes.TryGetValue(command, out var constructor))
                    throw new ZiPatchException();

                var chunk = constructor(reader, offset, innerSize - 5);

                return chunk;
            }
            catch (EndOfStreamException e)
            {
                throw new ZiPatchException("Could not get command", e);
            }
        }


        protected override void ReadChunk()
        {
            using var advanceAfter = new AdvanceOnDispose(this.Reader, Size);
        }

        protected SqpkChunk(ChecksumBinaryReader reader, long offset, long size) : base(reader, offset, size)
        { }

        public override string ToString()
        {
            return Type;
        }
    }
}