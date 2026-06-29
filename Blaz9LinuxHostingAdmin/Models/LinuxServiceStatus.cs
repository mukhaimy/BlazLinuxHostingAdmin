namespace Blaz9LinuxHostingAdmin.Models;

public sealed class LinuxServiceStatus
{
    public required LinuxServiceDefinition Service { get; init; }

    public string ActiveState { get; init; } = "unknown";

    public string SubState { get; init; } = "unknown";

    public string LoadState { get; init; } = "unknown";

    public string? SystemdDescription { get; init; }

    public string? MainPid { get; init; }

    public int? ExecMainStatus { get; init; }

    public bool CanStart => ActiveState is not "active" and not "activating";

    public bool CanStop => ActiveState is "active" or "activating";

    public bool IsActive => ActiveState == "active";
}
