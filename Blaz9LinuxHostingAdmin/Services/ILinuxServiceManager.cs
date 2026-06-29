using Blaz9LinuxHostingAdmin.Models;

namespace Blaz9LinuxHostingAdmin.Services;

public interface ILinuxServiceManager
{
    IReadOnlyList<LinuxServiceDefinition> GetConfiguredServices();

    Task<IReadOnlyList<LinuxServiceStatus>> GetStatusesAsync(CancellationToken cancellationToken = default);

    Task<LinuxServiceStatus> GetStatusAsync(string serviceName, CancellationToken cancellationToken = default);

    Task<ServiceCommandResult> StartAsync(string serviceName, CancellationToken cancellationToken = default);

    Task<ServiceCommandResult> StopAsync(string serviceName, CancellationToken cancellationToken = default);

    Task<ServiceCommandResult> RestartAsync(string serviceName, CancellationToken cancellationToken = default);

    Task<ServiceCommandResult> GetLogsAsync(string serviceName, int lineCount = 50, CancellationToken cancellationToken = default);
}
