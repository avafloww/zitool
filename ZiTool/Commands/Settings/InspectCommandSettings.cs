using System.ComponentModel;
using Spectre.Console.Cli;

namespace ZiTool.Commands.Settings;

public class InspectCommandSettings : CommandSettings
{
    [CommandArgument(0, "<filename>")]
    [Description("the target ZiPatch file")]
    public string Filename { get; set; }
    
    [CommandArgument(1, "[property]")]
    [Description("a single property to retrieve; if not set, a summary will be printed")]
    public string? Property { get; set; }
}
