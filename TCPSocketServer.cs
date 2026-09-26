using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.VisualBasic;
using reverse_proxy_server.RoutingService;

namespace reverse_proxy_server;

public partial class TCPSocketServer(
    List<RoutingTable> routingTables
)
{
    // private const string backendAddress = "127.0.0.1"; 
    // private const int backendPort = 5169;
    private const int proxyPort = 5000;


    public async Task Main()
    {

        var serverEndpoint = new IPEndPoint(
            address: IPAddress.Any,
            port: proxyPort
        );

        using var listenerSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp
        );

        listenerSocket.Bind(serverEndpoint);
        listenerSocket.Listen(backlog: 100);

        System.Console.WriteLine($"Server is listening at: {proxyPort}");

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
            var requestLine = await RoutingService.RoutingService.GetHeadersAsync(clientSocket);
            var requestPath = RoutingService.RoutingService.GetRequestPath(
                requestLine.buffer,
                requestLine.byteRead
            );
            var contentLength = RoutingService.RoutingService.GetContentLength(
                buffer: requestLine.buffer,
                byteRead: requestLine.byteRead
            ); // Thêm hàm lấy Content-length 

            var route = RoutingService.RoutingService.GetRoute(
                prefix: requestPath,
                routingTable: routingTables
            );
            
            using var backendSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            try
            {
                await backendSocket.ConnectAsync(
                    route.BackendHost,
                    route.BackendPort
                );

                System.Console.WriteLine($"Connected: {clientSocket.RemoteEndPoint} - {backendSocket.RemoteEndPoint}");

                await SendAllAsync(
                    backendSocket,
                    requestLine.buffer.AsMemory(0, requestLine.byteRead)
                );

                var clientToBackend = PumbAsync(clientSocket, backendSocket);
                var backendToClient = PumbAsync(backendSocket, clientSocket);

                await Task.WhenAny(clientToBackend, backendToClient);

            }
            catch(SocketException ex)
            {
                System.Console.WriteLine($"Socket error occur: {ex}");
            }

            finally
            {
                SafeShutdown(clientSocket);
                SafeShutdown(backendSocket);
            }
        }
    }
}
