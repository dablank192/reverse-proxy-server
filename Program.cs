using reverse_proxy_server;

public class Program
{
    async static Task Main()
    {
        var tcpServer = new TCPSocketServer();
        await tcpServer.Main();
    }
}