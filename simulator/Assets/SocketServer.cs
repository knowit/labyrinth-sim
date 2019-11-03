using System; 
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Google.Protobuf;
using UnityEngine;

public class ClientConnection
{
    private readonly Socket _client;

    public ClientConnection(Socket client, Action<GameUpdate> callback)
    {
        _client = client;
        Debug.Log($"Connected to {client.RemoteEndPoint.ToString()}");

        Task.Run(async () =>
        {
            var buffer = new byte[1024];
            while (true)
            {
                var bytes = _client.Receive(buffer, SocketFlags.None);
                if (bytes > 0)
                {
                    using (var ms = new MemoryStream(buffer))
                        callback(GameUpdate.Parser.ParseDelimitedFrom(ms));
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            };
        });
    }

    public async Task Send(GameUpdate update)
    {
        using (var ms = new MemoryStream())
        {
            update.WriteDelimitedTo(ms);
            var bytes = ms.ToArray();

            await Task.Run(() =>
            {
                _client.Send(bytes);
            });
        }
    }
};

public class SocketServer
{
    public async static Task<ClientConnection> Open(int port, Action<GameUpdate> callback)
    {
        var listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
        var localEndPoint = new IPEndPoint(IPAddress.Loopback, port);

        listener.Bind(localEndPoint);
        listener.Listen(100);

        return new ClientConnection(await listener.AcceptAsync(), callback);
    }
}
