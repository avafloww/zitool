using Spectre.Console;
using Spectre.Console.Cli;
using ZiPatchLib;
using ZiPatchLib.Util;
using ZiTool.Commands.Settings;

namespace ZiTool.Commands;

public class ApplyCommand : Command<ApplyCommandSettings>
{
    private ZiPatchFile? _zi;
    private SqexFileStreamStore? _store;

    public override int Execute(CommandContext context, ApplyCommandSettings settings)
    {
        var sourceInfo = new FileInfo(settings.Filename);
        if (!sourceInfo.Exists)
        {
            throw new FileNotFoundException("Provided source patch file does not exist");
        }

        var targetDir = new FileInfo(settings.TargetDirectory);
        Directory.CreateDirectory(targetDir.FullName);

        _zi = new ZiPatchFile(new SqexFileStream(sourceInfo.FullName, FileMode.Open));
        _store = new SqexFileStreamStore();

        var config = new ZiPatchConfig(targetDir.FullName)
        {
            Store = _store,
            IgnoreMissing = settings.IgnoreMissing,
            IgnoreOldMismatch = settings.IgnoreOldMismatch
        };

        if (settings.Verbose)
        {
            foreach (var chunk in _zi.GetChunks())
            {
                AnsiConsole.MarkupLine($"[yellow]Applying:[/] [green]{chunk}[/]");
                chunk.ApplyChunk(config);
            }
        }
        else
        {
            AnsiConsole.Status()
                .SpinnerStyle(Style.Parse("yellow"))
                .Start($"Applying patch: {sourceInfo.FullName}...", ctx =>
                {
                    foreach (var chunk in _zi.GetChunks())
                    {
                        chunk.ApplyChunk(config);
                    }
                });
        }

        return 0;
    }
}
