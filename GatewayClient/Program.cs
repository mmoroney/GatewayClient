using GatewayClient;
using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

Gateway? gateway = null;

string host = "gatewaymud.org";
int port = 6969;

Console.Title = "C# Gateway MUD Client";
Console.WriteLine($"--- Connecting to {host}:{port} ---");

try {
    var client = new TcpClient();
    await client.ConnectAsync(host, port);
    gateway = new Gateway(client);

    Console.WriteLine("--- Connected! (Press Ctrl+C to quit) ---\n");

    TriggerManager.EnableTrigger("OnHealthCheck", false, (string[] tokens) => {
        Console.WriteLine("Health checked");
    });

    TravelManager.Load();

    var readTask = Task.Run(ReadFromServer);

    while (gateway.IsConnected) {
        string? input = Console.ReadLine();
        if (string.IsNullOrEmpty(input)) continue;

        if (AliasManager.ProcessOutgoingLine(input)) {
            continue;
        }

        // Send the command to the MUD
        await Gateway.SendAsync(input);
    }
}
catch (Exception ex) {
    Console.WriteLine($"\n[ERROR] {ex.Message}");
}
finally {
    gateway?.Dispose();
}

async Task ReadFromServer() {
    byte[] buffer = new byte[4096];

    try {
        while (gateway != null && gateway.IsConnected) {
            // Wait for data from the server
            int bytesRead = await gateway.ReadAsync(buffer);

            if (bytesRead == 0) {
                Console.WriteLine("\n[INFO] Server closed the connection.");
                gateway.Disconnect();
                break;
            }

            // Convert bytes to string and display immediately
            string text = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.Write(text);
            TriggerManager.ProcessIncomingLine(text);
        }
    }
    catch (Exception) {
        if (gateway != null && gateway.IsConnected)
            Console.WriteLine("\n[INFO] Disconnected from server.");
    }
}

