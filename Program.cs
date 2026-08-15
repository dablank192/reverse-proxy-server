using reverse_proxy_server;
using reverse_proxy_server.RoutingService;

public class Program
{
    async static Task Main()
    {
        var routes = new List<RoutingTable>()
        {
            new()
            {
                PathPrefix = "/api/",
                BackendHost = "127.0.0.1",
                BackendPort = 5169
            }
        };
        
        var tcpServer = new TCPSocketServer(routingTables: routes);
        await tcpServer.Main();
    }
}