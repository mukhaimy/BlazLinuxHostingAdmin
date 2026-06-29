using Blaz9LinuxHostingAdmin.Models;

namespace Blaz9LinuxHostingAdmin.Options;

public sealed class LinuxServiceManagerOptions
{
    public List<LinuxServiceDefinition> Services { get; set; } = [];
}
