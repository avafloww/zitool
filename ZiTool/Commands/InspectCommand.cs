using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using ZiPatchLib;
using ZiPatchLib.Chunk;
using ZiPatchLib.Inspection;
using ZiPatchLib.Util;
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

        using var zi = new ZiPatchFile(new SqexFileStream(fileInfo.FullName, FileMode.Open));
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
                case "changes":
                    var changeset = zi.CalculateChangedFiles(new ZiPatchConfig(string.Empty));
                    AnsiConsole.WriteLine(JsonSerializer.Serialize(changeset));
                    return 0;
                default:
                    AnsiConsole.MarkupLine(
                        "[red]Unknown property.[/] [yellow]Valid properties:[/] [green]repository, type, minor, changes[/]");
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
            WriteHeader("Patch Repository");
            await WriteRepoInfo(header);

            WriteHeader("Patch Command Count");
            WriteCommandCounts(header.CommandCounts, zi.CalculateActualCounts());
        }

        WriteHeader("[green]+ Added[/]/[red]- Deleted[/]/[yellow]* Modified[/] Files");
        WriteChangedFiles(zi);

        return 0;
    }

    private async Task WriteRepoInfo(FileHeaderChunk header)
    {
        var hash = header.RepositoryName.ToString("x8");

        WriteAttributeName("Hash");
        WriteAttributeValue(hash);

        if (header.RepositoryName == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No repository ID set in patch file.[/]");
        }
        else
        {
            var repositories = await _thaliak.GetRepositories();
            var repo = repositories.FirstOrDefault(repo => repo.Slug == hash);

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

    private void WriteCommandCountValue(uint fhdr, uint actual)
    {
        WriteAttributeValue(fhdr == actual ? fhdr : $"{actual} [gray](FHDR: [/][yellow]{fhdr}[/][gray])[/]");
    }

    private void WriteCommandCounts(ZiPatchCommandCounts fhdr, ZiPatchCommandCounts actual)
    {
        WriteAttributeName("Total Commands");
        WriteCommandCountValue(fhdr.TotalCommands, actual.TotalCommands);

        WriteAttributeName("ADIR   (Add Directory)");
        WriteCommandCountValue(fhdr.AddDirectories, actual.AddDirectories);

        WriteAttributeName("DELD   (Delete Directory)");
        WriteCommandCountValue(fhdr.DeleteDirectories, actual.DeleteDirectories);

        WriteAttributeName("SQPK:H (SqPack Header)");
        WriteCommandCountValue(fhdr.SqpkHeaderCommands, actual.SqpkHeaderCommands);

        WriteAttributeName("SQPK:F (SqPack File Operation)");
        WriteCommandCountValue(fhdr.SqpkFileCommands, actual.SqpkFileCommands);

        WriteAttributeName("SQPK:A (SqPack Data Add)");
        WriteCommandCountValue(fhdr.SqpkAddCommands, actual.SqpkAddCommands);

        WriteAttributeName("SQPK:D (SqPack Data Delete)");
        WriteCommandCountValue(fhdr.SqpkDeleteCommands, actual.SqpkDeleteCommands);

        WriteAttributeName("SQPK:E (SqPack Data Expand)");
        WriteCommandCountValue(fhdr.SqpkExpandCommands, actual.SqpkExpandCommands);
    }

    private void WriteChangedFiles(ZiPatchFile zi)
    {
        var changes = zi.CalculateChangedFiles(new ZiPatchConfig(string.Empty));
        foreach (var file in changes.Added)
        {
            AnsiConsole.MarkupLine($"    [green]+ {file}[/]");
        }

        foreach (var file in changes.Deleted)
        {
            AnsiConsole.MarkupLine($"    [red]- {file}[/]");
        }

        foreach (var file in changes.Modified)
        {
            AnsiConsole.MarkupLine($"    [yellow]* {file}[/]");
        }
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
