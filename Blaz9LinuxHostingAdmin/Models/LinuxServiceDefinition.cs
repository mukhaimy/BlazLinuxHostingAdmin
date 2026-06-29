namespace Blaz9LinuxHostingAdmin.Models;

public sealed class LinuxServiceDefinition
{
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }
}
