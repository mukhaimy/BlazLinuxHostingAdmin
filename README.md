# BlazLinuxHostingAdmin

Aplikasi Blazor untuk melihat status, start, stop, restart, dan membaca log service `.NET` yang berjalan sebagai `systemd` service di Ubuntu Linux.

## Konfigurasi service

Daftarkan service yang boleh dikelola di `Blaz9LinuxHostingAdmin/appsettings.json`:

```json
"LinuxServiceManager": {
  "Services": [
    {
      "Name": "my-dotnet-app.service",
      "DisplayName": "My .NET App",
      "Description": "Aplikasi .NET production"
    }
  ]
}
```

Halaman admin tersedia di `/services` setelah login.

## Permission Ubuntu

User Linux yang menjalankan aplikasi Blazor harus punya izin menjalankan:

```bash
systemctl show <service>
systemctl start <service>
systemctl stop <service>
systemctl restart <service>
journalctl -u <service>
```

Untuk production, batasi izin hanya ke unit service yang didaftarkan. Jangan beri akses `systemctl` umum tanpa pembatasan.
