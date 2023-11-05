using ZiPatchLib.Util;

namespace ZiPatchLib.Chunk
{
    public class DeleteDirectoryChunk : ZiPatchChunk
    {
        public new static string Type = "DELD";

        public string DirName { get; protected set; }

        public DeleteDirectoryChunk(ChecksumBinaryReader reader, long offset, long size) : base(reader, offset, size) {}

        protected override void ReadChunk()
        {
            using var advanceAfter = new AdvanceOnDispose(this.Reader, Size);
            var dirNameLen = this.Reader.ReadUInt32BE();

            DirName = this.Reader.ReadFixedLengthString(dirNameLen);
        }

        public override void ApplyChunk(ZiPatchConfig config)
        {
            Directory.Delete(config.GamePath + DirName);
        }

        public override string ToString()
        {
            return $"{Type}:{DirName}";
        }
    }
}