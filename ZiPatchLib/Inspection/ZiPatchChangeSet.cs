namespace ZiPatchLib.Inspection;

public class ZiPatchChangeSet
{
    public IEnumerable<string> Added { get; init; }
    public IEnumerable<string> Deleted { get; init; }
    public IEnumerable<string> Modified { get; init; }
}
