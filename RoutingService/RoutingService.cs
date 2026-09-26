using System;
using System.Net.Sockets;
using System.Text;

namespace reverse_proxy_server.RoutingService;

public static class RoutingService
{
    public static string GetRequestPath(byte[] buffer, int byteRead)
    {
        var requestText = Encoding.ASCII.GetString(
            buffer,
            0,  
            byteRead
        );

        string firstLine = requestText.Split("\r\n")[0];

        var parts = firstLine.Split(' ');
        var prefix = parts[1];

        return prefix;
    }

    public static async Task<(byte[] buffer, int byteRead)> GetHeadersAsync(Socket clientSocket)
    {
        byte[] buffer = new byte[9000];

        int totalByteRead = 0;


        while (true)
        {
            var byteRead = await clientSocket.ReceiveAsync(buffer.AsMemory(totalByteRead), SocketFlags.None);
            
            if(byteRead == 0) throw new IOException("Client close connection");

            totalByteRead += byteRead;

            var validPrefix = Encoding.ASCII.GetString(
                buffer,
                0,
                totalByteRead
            );

            if (validPrefix.Contains("\r\n\r\n"))
            {
                return (buffer, totalByteRead);
            }

            if(totalByteRead == buffer.Length)
            {
                throw new Exception("Request line is too large");
            }
        }
    }

    public static RoutingTable GetRoute(string prefix, List<RoutingTable> routingTable)
    {
        var routes = routingTable.OrderByDescending(t => t.PathPrefix.Length).ToList();

        var result = routes.FirstOrDefault(
            t => prefix.StartsWith(
                t.PathPrefix,
                StringComparison.OrdinalIgnoreCase
                )) ?? throw new Exception("Route not Found");
 
        return result;
    }

    public static int GetContentLength(byte[] buffer, int byteRead)
    {
        var request = Encoding.ASCII.GetString(
            bytes: buffer,
            index: 0,
            count: byteRead
        );

        var headerEnd = request.IndexOf("\r\n\r\n");

        var header = request[..headerEnd];

        var lines = header.Split(separator: "\r\n");

        var contentLengthLine = lines.FirstOrDefault(
            t => t.StartsWith("Content-Length:",
            StringComparison.OrdinalIgnoreCase)
        );

        if(contentLengthLine == null) return 0;

        var length = contentLengthLine.Split(":")[1].Trim();

        return int.Parse(length);
    }
}
