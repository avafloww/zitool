using Spectre.Console;
using Spectre.Console.Cli;
using ZiPatchLib;
using ZiPatchLib.Chunk;
using ZiTool.Commands.Settings;
using ZiTool.Thaliak;
using ZiTool.Util;

namespace ZiTool.Commands;

public class InspectCommand : AsyncCommand<InspectCommandSettings>
{
    private readonly ThaliakClient _thaliak;

    public InspectCommand(ThaliakClient thaliak)
    {
        _thaliak = thaliak;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, InspectCommandSettings settings)
    {
        var fileInfo = new FileInfo(settings.Filename);

        using var zi = ZiPatchFile.FromFileName(fileInfo.FullName);
        FileHeaderChunk? header = null;
        foreach (var chunk in zi.GetChunks())
        {
            if (chunk is FileHeaderChunk fhdr)
            {
                header = fhdr;
                break;
            }
        }

        if (header == null)
        {
            AnsiConsole.MarkupLine("[red]Could not find FHDR chunk![/]");
            return 1;
        }

        if (settings.Property != null)
        {
            switch (settings.Property.ToLowerInvariant())
            {
                case "repository":
                case "repo":
                    AnsiConsole.WriteLine(header.RepositoryName.ToString("x8"));
                    return 0;
                case "type":
                    AnsiConsole.WriteLine(header.PatchType);
                    return 0;
                case "minor":
                    AnsiConsole.WriteLine(header.MinorVersion);
                    return 0;
                default:
                    AnsiConsole.MarkupLine(
                        "[red]Unknown property.[/] [yellow]Valid properties:[/] [green]repository, type, minor[/]");
                    return 1;
            }
        }

        WriteHeader("Input File");

        WriteAttributeName("Path");
        AnsiConsole.Write(new TextPath(fileInfo.FullName));

        WriteAttributeName("Size");
        WriteAttributeValue(fileInfo.GetHumanSize());

        WriteHeader("Patch Metadata");
        WritePatchMeta(header);

        if (header.Version >= 3)
        {
            WriteHeader("Patch Command Count");
            WriteCommandCounts(header);

            WriteHeader("Patch Repository");
            await WriteRepoInfo(header);
        }

        return 0;
    }

    private async Task WriteRepoInfo(FileHeaderChunk header)
    {
        var repositories = await _thaliak.GetRepositories();
        var hash = header.RepositoryName.ToString("x8");
        var repo = repositories.FirstOrDefault(repo => repo.Slug == hash);

        WriteAttributeName("Hash");
        WriteAttributeValue(hash);

        WriteAttributeName("Name");
        if (repo != null)
        {
            WriteAttributeValue(repo.Name);

            WriteAttributeName("Description");
            WriteAttributeValue(repo.Description);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]unknown[/]");
            AnsiConsole.MarkupLine("[yellow]No matching repository hash found in Thaliak.[/]");
        }
    }

    private void WritePatchMeta(FileHeaderChunk header)
    {
        WriteAttributeName("ZiPatch Version");
        WriteAttributeValue(header.Version);

        WriteAttributeName("Patch Type");
        WriteAttributeValue(header.PatchType);

        WriteAttributeName("Entry Files");
        WriteAttributeValue(header.EntryFiles);

        if (header.Version >= 3)
        {
            WriteAttributeName("Minor Version");
            WriteAttributeValue(header.MinorVersion);

            WriteAttributeName("Deleted Data Size");
            WriteAttributeValue(SizeUtil.GetBytesReadable(header.DeleteDataSize));
        }
    }

    private void WriteCommandCounts(FileHeaderChunk header)
    {
        WriteAttributeName("Total Commands");
        WriteAttributeValue(header.Commands);

        WriteAttributeName("ADIR   (Add Directory)");
        WriteAttributeValue(header.AddDirectories);

        WriteAttributeName("DELD   (Delete Directory)");
        WriteAttributeValue(header.DeleteDirectories);

        WriteAttributeName("SQPK:H (SqPack Header)");
        WriteAttributeValue(header.SqpkHeaderCommands);

        WriteAttributeName("SQPK:F (SqPack File Operation)");
        WriteAttributeValue(header.SqpkFileCommands);

        WriteAttributeName("SQPK:A (SqPack Data Add)");
        WriteAttributeValue(header.SqpkAddCommands);

        WriteAttributeName("SQPK:D (SqPack Data Delete)");
        WriteAttributeValue(header.SqpkDeleteCommands);

        WriteAttributeName("SQPK:E (SqPack Data Expand)");
        WriteAttributeValue(header.SqpkExpandCommands);
    }

    private void WriteHeader(string title)
    {
        AnsiConsole.Write(new Rule
        {
            Alignment = Justify.Left,
            Style = Style.Parse("aqua"),
            Title = title
        });
    }

    private void WriteAttributeName(string name)
    {
        AnsiConsole.Markup($"    [yellow]{name}[/]: ");
    }

    private void WriteAttributeValue(object value)
    {
        AnsiConsole.MarkupLine($"[green]{value}[/]");
    }
}
