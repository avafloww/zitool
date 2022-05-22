﻿namespace ZiPatchLib.Util;

internal class SqpackIndexFile : SqpackFile
{
    public SqpackIndexFile(BinaryReader reader) : base(reader) { }


    protected override string GetFileName(ZiPatchConfig.PlatformId platform)
    {
        return $"{base.GetFileName(platform)}.index{(FileId == 0 ? string.Empty : FileId.ToString())}";
    }
}
