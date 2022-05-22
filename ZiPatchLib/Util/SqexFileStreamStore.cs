namespace ZiPatchLib.Util;

public class SqexFileStreamStore : IDisposable
{
    private readonly Dictionary<string, SqexFileStream> _streams = new();

    public void Dispose()
    {
        foreach (var stream in _streams.Values)
        {
            stream.Dispose();
        }
    }

    public SqexFileStream GetStream(string path, FileMode mode, int tries, int sleeptime)
    {
        // Normalise path
        path = Path.GetFullPath(path);

        if (_streams.TryGetValue(path, out var stream))
        {
            return stream;
        }

        stream = SqexFileStream.WaitForStream(path, mode, tries, sleeptime);
        _streams.Add(path, stream);

        return stream;
    }
}
