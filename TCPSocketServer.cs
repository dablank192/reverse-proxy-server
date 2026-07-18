using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace reverse_proxy_server;

public class TCPSocketServer
{
    public async Task Main()
    {
        var ipAddress = "0.0.0.0";
        var port = 5000;

        var serverEndpoint = new IPEndPoint(
            address: IPAddress.Parse(ipAddress),
            port: port
        );

        using var listenerSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp
        );

        listenerSocket.Bind(serverEndpoint);
        listenerSocket.Listen(backlog: 100);

        System.Console.WriteLine($"Server is listening at: {port}");

        while (true)
        {
            var clientSocket = await listenerSocket.AcceptAsync();

            System.Console.WriteLine($"Open connection to: {clientSocket.RemoteEndPoint}");

            _ = HandleClientAsync(clientSocket);
        }
    }

    public async Task HandleClientAsync(Socket clientSocket)
    {
        using (clientSocket)
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[4000];

                    var receivedByte = await clientSocket.ReceiveAsync(
                        buffer,
                        SocketFlags.None
                    );

                    if(receivedByte == 0)
                    {
                        System.Console.WriteLine("Client disconnected");

                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, receivedByte);

                    System.Console.WriteLine($"Client's message: {message}");

                    var response = Encoding.UTF8.GetBytes($"Echo:{message}");

                    await clientSocket.SendAsync(response, SocketFlags.None);
                }
            }
            catch(SocketException ex)
            {
                System.Console.WriteLine($"Socket Error occur: {ex}");
            }

        }
    }
}
