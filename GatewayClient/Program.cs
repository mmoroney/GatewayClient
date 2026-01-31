using GatewayClient;
using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

TcpClient? _client = null;
NetworkStream? _stream = null;
bool _isConnected = false;

string host = "gatewaymud.org";
int port = 6969;

Console.Title = "C# Gateway MUD Client";
Console.WriteLine($"--- Connecting to {host}:{port} ---");

try {
    _client = new TcpClient();
    await _client.ConnectAsync(host, port);
    _stream = _client.GetStream();
    _isConnected = true;

    Console.WriteLine("--- Connected! (Press Ctrl+C to quit) ---\n");

    TriggerManager.EnableTrigger("OnHealthCheck", false, (string[] tokens) => {
        Console.WriteLine("Health checked");
    });

    var readTask = Task.Run(ReadFromServer);

    while (_isConnected) {
        string? input = Console.ReadLine();
        if (string.IsNullOrEmpty(input)) continue;

        // Send the command + newline to the MUD
        byte[] data = Encoding.ASCII.GetBytes(input + "\n");
        await _stream.WriteAsync(data);
    }
}
catch (Exception ex) {
    Console.WriteLine($"\n[ERROR] {ex.Message}");
}
finally {
    _isConnected = false;
    _stream?.Close();
    _client?.Close();
}

async Task ReadFromServer() {
    byte[] buffer = new byte[4096];

    try {
        while (_isConnected) {
            // Wait for data from the server
            int bytesRead = await _stream.ReadAsync(buffer);

            if (bytesRead == 0) {
                Console.WriteLine("\n[INFO] Server closed the connection.");
                _isConnected = false;
                break;
            }

            // Convert bytes to string and display immediately
            string text = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.Write(text);
            TriggerManager.ProcessIncomingLine(text);
        }
    }
    catch (Exception) {
        if (_isConnected)
            Console.WriteLine("\n[INFO] Disconnected from server.");
    }
}

