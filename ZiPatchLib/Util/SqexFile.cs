﻿namespace ZiPatchLib.Util;

public class SqexFile
{
    protected SqexFile() { }

    public SqexFile(string relativePath)
    {
        RelativePath = relativePath;
    }

    public string RelativePath { get; set; }

    public SqexFileStream? OpenStream(string basePath, FileMode mode, int tries = 5, int sleeptime = 1)
    {
        return SqexFileStream.WaitForStream($@"{basePath}/{RelativePath}", mode, tries, sleeptime);
    }

    public SqexFileStream OpenStream(SqexFileStreamStore store, string basePath, FileMode mode,
        int tries = 5, int sleeptime = 1)
    {
        return store.GetStream($@"{basePath}/{RelativePath}", mode, tries, sleeptime);
    }

    public void CreateDirectoryTree(string basePath)
    {
        var dirName = Path.GetDirectoryName($@"{basePath}/{RelativePath}");
        if (dirName != null)
        {
            Directory.CreateDirectory(dirName);
        }
    }

    public override string ToString()
    {
        return RelativePath;
    }

    public static string GetExpansionFolder(byte expansionId)
    {
        return expansionId == 0 ? "ffxiv" : $"ex{expansionId}";
    }

    public static IEnumerable<string> GetAllExpansionFiles(string fullPath, ushort expansionId)
    {
        var xpacPath = GetExpansionFolder((byte) expansionId);

        var sqpack = $@"{fullPath}\sqpack\{xpacPath}";
        var movie = $@"{fullPath}\movie\{xpacPath}";

        var files = Enumerable.Empty<string>();

        if (Directory.Exists(sqpack))
        {
            files = files.Concat(Directory.GetFiles(sqpack));
        }

        if (Directory.Exists(movie))
        {
            files = files.Concat(Directory.GetFiles(movie));
        }

        return files;
    }
}
