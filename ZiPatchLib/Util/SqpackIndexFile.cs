using System.IO;

namespace ZiPatchLib.Util
{
    class SqpackIndexFile : SqpackFile
    {
        public SqpackIndexFile(BinaryReader reader) : base(reader) {}


        public override string GetFileName(ZiPatchConfig.PlatformId platform) =>
            $"{base.GetFileName(platform)}.index{(FileId == 0 ? string.Empty : FileId.ToString())}";
    }
}