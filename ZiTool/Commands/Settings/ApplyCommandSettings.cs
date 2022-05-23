using System.ComponentModel;
using Spectre.Console.Cli;

namespace ZiTool.Commands.Settings;

public class ApplyCommandSettings : CommandSettings
{
    [CommandArgument(0, "<filename>")]
    [Description("the target ZiPatch file")]
    public string Filename { get; set; }

    [CommandArgument(1, "<target>")]
    [Description("the target directory to apply this patch to")]
    public string TargetDirectory { get; set; }

    [CommandOption("-v|--verbose")]
    [Description("if enabled, ZiPatch operations during the apply process will be logged")]
    public bool Verbose { get; set; }

    [CommandOption("--ignore-missing")]
    [Description("ignores missing files")]
    public bool IgnoreMissing { get; set; }

    [CommandOption("--ignore-old-mismatch")]
    [Description("ignores old file mismatches")]
    public bool IgnoreOldMismatch { get; set; }
}
