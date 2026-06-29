namespace Blaz9LinuxHostingAdmin.Models;

public sealed class ServiceCommandResult
{
    public bool Succeeded { get; init; }

    public int ExitCode { get; init; }

    public string Output { get; init; } = string.Empty;

    public string Error { get; init; } = string.Empty;

    public static ServiceCommandResult Success(string output = "") => new()
    {
        Succeeded = true,
        Output = output
    };
}
