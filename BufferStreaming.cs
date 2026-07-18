using System;
using System.Net.Sockets;

namespace reverse_proxy_server;

public partial class TCPSocketServer
{
    public async Task PumbAsync(
        Socket source,
        Socket destination)
    {
        byte[] buffer = new byte[9000];

        while (true)
        {
            var receivedByte = await source.ReceiveAsync(buffer, SocketFlags.None);

            if (receivedByte == 0)
            {
                try
                {
                    destination.Shutdown(SocketShutdown.Send);
                }
                catch{}

                break;
            }

            await SendAllAsync(destination, buffer.AsMemory(0, receivedByte));
        }
    }

    public async Task SendAllAsync(Socket socket, ReadOnlyMemory<byte> data)
    {
        int totalSent = 0;

        while(totalSent < data.Length)
        {
            var remainingData = data.Slice(totalSent);

            var sentData = await socket.SendAsync(remainingData, SocketFlags.None);

            if(sentData == 0) throw new IOException("Socket can not send data");

            totalSent += sentData;
        }
    }

    public static void SafeShutdown(Socket socket)
    {
        try
        {
            socket.Shutdown(SocketShutdown.Both);
        }
        catch
        {
            
        }
    }
}
