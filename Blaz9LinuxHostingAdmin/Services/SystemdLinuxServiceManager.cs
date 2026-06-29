using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Blaz9LinuxHostingAdmin.Models;
using Blaz9LinuxHostingAdmin.Options;
using Microsoft.Extensions.Options;

namespace Blaz9LinuxHostingAdmin.Services;

public sealed partial class SystemdLinuxServiceManager(IOptions<LinuxServiceManagerOptions> options, ILogger<SystemdLinuxServiceManager> logger) : ILinuxServiceManager
{
    private const int DefaultTimeoutSeconds = 20;
    private readonly IReadOnlyList<LinuxServiceDefinition> services = options.Value.Services
        .Where(service => !string.IsNullOrWhiteSpace(service.Name))
        .Select(service => new LinuxServiceDefinition
        {
            Name = NormalizeServiceName(service.Name),
            DisplayName = string.IsNullOrWhiteSpace(service.DisplayName) ? NormalizeServiceName(service.Name) : service.DisplayName,
            Description = service.Description
        })
        .ToArray();

    public IReadOnlyList<LinuxServiceDefinition> GetConfiguredServices() => services;

    public async Task<IReadOnlyList<LinuxServiceStatus>> GetStatusesAsync(CancellationToken cancellationToken = default)
    {
        var statuses = new List<LinuxServiceStatus>();

        foreach (var service in services)
        {
            statuses.Add(await GetStatusAsync(service.Name, cancellationToken));
        }

        return statuses;
    }

    public async Task<LinuxServiceStatus> GetStatusAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        var service = GetAllowedService(serviceName);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new LinuxServiceStatus
            {
                Service = service,
                ActiveState = "unavailable",
                SubState = "not-linux",
                LoadState = "unavailable",
                SystemdDescription = "systemctl hanya tersedia saat aplikasi berjalan di Linux."
            };
        }

        var result = await RunCommandAsync("systemctl", [
            "show",
            service.Name,
            "--property=ActiveState",
            "--property=SubState",
            "--property=LoadState",
            "--property=Description",
            "--property=MainPID",
            "--property=ExecMainStatus",
            "--no-pager"
        ], cancellationToken);

        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to read status for {ServiceName}: {Error}", service.Name, result.Error);

            return new LinuxServiceStatus
            {
                Service = service,
                ActiveState = "unknown",
                SubState = "error",
                LoadState = "unknown",
                SystemdDescription = FirstNonEmpty(result.Error, result.Output)
            };
        }

        var values = ParseSystemctlShow(result.Output);

        return new LinuxServiceStatus
        {
            Service = service,
            ActiveState = GetRequiredValue(values, "ActiveState", "unknown"),
            SubState = GetRequiredValue(values, "SubState", "unknown"),
            LoadState = GetRequiredValue(values, "LoadState", "unknown"),
            SystemdDescription = GetValue(values, "Description", service.Description),
            MainPid = GetValue(values, "MainPID", null),
            ExecMainStatus = int.TryParse(GetValue(values, "ExecMainStatus", null), out var exitCode) ? exitCode : null
        };
    }

    public Task<ServiceCommandResult> StartAsync(string serviceName, CancellationToken cancellationToken = default) =>
        RunSystemctlActionAsync("start", serviceName, cancellationToken);

    public Task<ServiceCommandResult> StopAsync(string serviceName, CancellationToken cancellationToken = default) =>
        RunSystemctlActionAsync("stop", serviceName, cancellationToken);

    public Task<ServiceCommandResult> RestartAsync(string serviceName, CancellationToken cancellationToken = default) =>
        RunSystemctlActionAsync("restart", serviceName, cancellationToken);

    public Task<ServiceCommandResult> GetLogsAsync(string serviceName, int lineCount = 50, CancellationToken cancellationToken = default)
    {
        var service = GetAllowedService(serviceName);
        var safeLineCount = Math.Clamp(lineCount, 10, 200);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Task.FromResult(ServiceCommandResult.Success("Log service hanya tersedia saat aplikasi berjalan di Linux."));
        }

        return RunCommandAsync("journalctl", [
            "-u",
            service.Name,
            "-n",
            safeLineCount.ToString(),
            "--no-pager",
            "--output=short-iso"
        ], cancellationToken);
    }

    private async Task<ServiceCommandResult> RunSystemctlActionAsync(string action, string serviceName, CancellationToken cancellationToken)
    {
        var service = GetAllowedService(serviceName);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new ServiceCommandResult
            {
                Succeeded = false,
                Error = "Perintah systemctl hanya bisa dijalankan di Ubuntu/Linux."
            };
        }

        return await RunCommandAsync("systemctl", [action, service.Name], cancellationToken);
    }

    private LinuxServiceDefinition GetAllowedService(string serviceName)
    {
        var normalizedName = NormalizeServiceName(serviceName);

        if (!SafeServiceNameRegex().IsMatch(normalizedName))
        {
            throw new InvalidOperationException("Nama service tidak valid.");
        }

        return services.FirstOrDefault(service => string.Equals(service.Name, normalizedName, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Service '{normalizedName}' tidak terdaftar di konfigurasi.");
    }

    private static async Task<ServiceCommandResult> RunCommandAsync(string fileName, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(DefaultTimeoutSeconds), cancellationToken);
        var exitTask = process.WaitForExitAsync(cancellationToken);
        var completedTask = await Task.WhenAny(exitTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }

            return new ServiceCommandResult
            {
                Succeeded = false,
                ExitCode = -1,
                Error = $"Command '{fileName}' timeout setelah {DefaultTimeoutSeconds} detik."
            };
        }

        var output = await outputTask;
        var error = await errorTask;

        return new ServiceCommandResult
        {
            Succeeded = process.ExitCode == 0,
            ExitCode = process.ExitCode,
            Output = output.Trim(),
            Error = error.Trim()
        };
    }

    private static Dictionary<string, string> ParseSystemctlShow(string output)
    {
        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeServiceName(string serviceName)
    {
        var trimmed = serviceName.Trim();
        return trimmed.EndsWith(".service", StringComparison.Ordinal) ? trimmed : $"{trimmed}.service";
    }

    private static string? GetValue(IReadOnlyDictionary<string, string> values, string key, string? fallback) =>
        values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;

    private static string GetRequiredValue(IReadOnlyDictionary<string, string> values, string key, string fallback) =>
        values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    [GeneratedRegex(@"^[A-Za-z0-9_.@:-]+\.service$")]
    private static partial Regex SafeServiceNameRegex();
}
