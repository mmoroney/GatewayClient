using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayClient {
    public class Gateway : IDisposable {
        private static Gateway? _instance;

        public static Gateway Instance =>
            _instance ?? throw new InvalidOperationException("Gateway is not connected or initialized.");

        public static bool IsInitialized => _instance is { IsConnected: true };

        public static void Send(string command) => Instance.SendCommand(command);

        public static Task SendAsync(string command, CancellationToken ct = default) =>
            Instance.SendCommandAsync(command, ct);

        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly SemaphoreSlim _writeLock = new(1, 1);
        private bool _isConnected;
        private bool _disposed;

        public bool IsConnected => _isConnected && _client.Connected;

        public Gateway(TcpClient client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _stream = client.GetStream();
            _isConnected = true;
            _instance = this;
        }

        public async Task SendCommandAsync(string command, CancellationToken ct = default) {
            if (!IsConnected) {
                return;
            }

            byte[] data = Encoding.ASCII.GetBytes(command.TrimEnd() + "\n");
            await _writeLock.WaitAsync(ct).ConfigureAwait(false);
            try {
                if (!IsConnected) {
                    return;
                }
                await _stream.WriteAsync(data, ct).ConfigureAwait(false);
            }
            finally {
                _writeLock.Release();
            }
        }

        public void SendCommand(string command) {
            _ = Task.Run(async () => {
                try {
                    await SendCommandAsync(command);
                }
                catch (Exception ex) {
                    Console.WriteLine($"\n[ERROR] Failed to send command: {ex.Message}");
                }
            });
        }

        public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default) {
            return _stream.ReadAsync(buffer, ct);
        }

        public void Disconnect() {
            if (!_isConnected) return;
            _isConnected = false;

            try { _stream.Close(); } catch { }
            try { _client.Close(); } catch { }
        }

        public void Dispose() {
            if (_disposed) return;
            _disposed = true;

            Disconnect();
            _writeLock.Dispose();

            if (_instance == this) {
                _instance = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
