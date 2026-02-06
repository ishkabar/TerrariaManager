using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;

namespace Ogur.Terraria.Manager.Infrastructure.Services;

public class SshService : IDisposable
{
    private SshClient? _sshClient;
    private ShellStream? _shellStream;
    private bool _isAttached;

    public event EventHandler<string>? OutputReceived;
    public event EventHandler<bool>? ConnectionStateChanged;

    public bool IsConnected => _sshClient?.IsConnected ?? false;
    public bool IsAttached => _isAttached;

    private CancellationTokenSource? _readCancellation;
    private string? _containerName;

    public async Task<bool> ConnectAsync(string host, int port, string username, string password)
    {
        return await Task.Run(() =>
        {
            try
            {
                Disconnect();

                _sshClient = new SshClient(host, port, username, password);
                _sshClient.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);

                _sshClient.Connect();

                ConnectionStateChanged?.Invoke(this, true);
                return true;
            }
            catch (Renci.SshNet.Common.SshAuthenticationException ex)
            {
                Console.WriteLine($"❌ SSH Authentication failed: {ex.Message}");
                OutputReceived?.Invoke(this, $"❌ Authentication failed: Invalid username or password\n");
                ConnectionStateChanged?.Invoke(this, false);
                return false;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Console.WriteLine($"❌ SSH Connection failed: {ex.Message}");
                OutputReceived?.Invoke(this, $"❌ Connection failed: Cannot reach host {host}:{port}\n");
                ConnectionStateChanged?.Invoke(this, false);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SSH Connection failed: {ex.Message}");
                OutputReceived?.Invoke(this, $"❌ Connection failed: {ex.Message}\n");
                ConnectionStateChanged?.Invoke(this, false);
                return false;
            }
        });
    }



    public async Task<bool> AttachToDockerAsync(string containerName)
    {
        if (!IsConnected) return false;

        try
        {
            _containerName = containerName;
            _shellStream = _sshClient!.CreateShellStream("xterm-256color", 120, 40, 800, 600, 4096);

            await Task.Delay(1000); // poczekaj na shell prompt

            // Flush MOTD
            if (_shellStream.DataAvailable)
            {
                _shellStream.Read();
            }

            // docker exec z interaktywnym shellem podłączonym do terraria procesu
            var cmd = $"docker attach {containerName}\n";
            var attachBytes = new UTF8Encoding(false).GetBytes(cmd);
            await _shellStream.WriteAsync(attachBytes, 0, attachBytes.Length);
            await _shellStream.FlushAsync();

            await Task.Delay(2000);
            _isAttached = true;
            StartReadingOutput();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Attach failed: {ex.Message}");
            return false;
        }
    }

    public async Task SendCommandAsync(string command)
    {
        if (_sshClient == null || !_sshClient.IsConnected || string.IsNullOrEmpty(_containerName))
            return;

        try
        {
            Console.WriteLine($"📤 SENDING: '{command}'");

            var escaped = command.Replace("\"", "\\\"");
            var cmd = $"echo \"{escaped}\" | docker exec -i {_containerName} /bin/sh -c 'cat > /proc/1/fd/0'";

            Console.WriteLine($"📤 CMD: {cmd}");

            var result = await Task.Run(() => _sshClient.RunCommand(cmd));
            Console.WriteLine($"✅ SENT: exit={result.ExitStatus}, err='{result.Error}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Send failed: {ex.Message}");
        }
    }

    private void StartReadingOutput()
    {
        _readCancellation?.Cancel();
        _readCancellation = new CancellationTokenSource();

        Task.Run(async () => await ReadOutputLoopAsync(_readCancellation.Token), _readCancellation.Token);
    }

    private async Task ReadOutputLoopAsync(CancellationToken cancellationToken)
    {
        if (_shellStream == null)
        {
            Console.WriteLine("❌ Read loop - shellStream is null!");
            return;
        }

        Console.WriteLine("✅ Read loop STARTED");

        var reader = new StreamReader(_shellStream, Encoding.UTF8);
        var buffer = new char[1024];

        try
        {
            while (!cancellationToken.IsCancellationRequested && _shellStream.CanRead)
            {
                Console.WriteLine("⏳ Waiting for data...");

                var read = await reader.ReadAsync(buffer, 0, buffer.Length);

                Console.WriteLine($"📊 Read {read} chars from stream");

                if (read > 0)
                {
                    var output = new string(buffer, 0, read);
                    Console.WriteLine($"📥 RAW: {output}");

                    output = StripAnsiCodes(output);
                    Console.WriteLine($"📥 CLEANED: {output}");

                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        OutputReceived?.Invoke(this, output);
                    }
                }

                await Task.Delay(50, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Read loop error: {ex}");
        }

        Console.WriteLine("🛑 Read loop ENDED");
    }

    private string StripAnsiCodes(string text)
    {
        // Remove ANSI escape sequences
        var pattern = @"\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])";
        var result = Regex.Replace(text, pattern, "");

        // Remove other control chars except newline/tab
        result = Regex.Replace(result, @"[\x00-\x08\x0B-\x0C\x0E-\x1F]", "");

        return result;
    }

    public void Disconnect()
    {
        _readCancellation?.Cancel();
        _isAttached = false;
        _containerName = null;

        _shellStream?.Dispose();
        _shellStream = null;

        if (_sshClient?.IsConnected == true)
        {
            _sshClient.Disconnect();
        }

        _sshClient?.Dispose();
        _sshClient = null;

        ConnectionStateChanged?.Invoke(this, false);
    }

    public void Dispose()
    {
        Disconnect();
    }
}