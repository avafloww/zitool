using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk
{
    public class EndOfFileChunk : ZiPatchChunk
    {
        public new static string Type = "EOF_";

        protected override void ReadChunk()
        {
            using var advanceAfter = new AdvanceOnDispose(this.Reader, Size);
        }

        public EndOfFileChunk(ChecksumBinaryReader reader, long offset, long size) : base(reader, offset, size) {}

        public override string ToString()
        {
            return Type;
        }
    }
}