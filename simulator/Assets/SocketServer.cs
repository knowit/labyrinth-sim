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
    private readonly Action _onClose;

    public ClientConnection(Socket client, Action<GameUpdate> onData, Action onClose)
    {
        _client = client;
        _onClose = onClose;
        Debug.Log($"Connected to {client.RemoteEndPoint.ToString()}");
        
        Task.Run(async () =>
        {
            var buffer = new byte[1024];
            while (true)
            {
                if (!_client.Connected)
                {
                    _onClose();
                    return;
                }

                var bytes = _client.Receive(buffer, SocketFlags.None);
                if (bytes > 0)
                {
                    using (var ms = new MemoryStream(buffer))
                        onData(GameUpdate.Parser.ParseDelimitedFrom(ms));
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            };
        });
    }

    public async Task Send(GameUpdate update)
    {
        if (!_client.Connected)
        {
            _onClose();
            return;
        }

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
    private readonly Socket _listener;

    public SocketServer(int port)
    {
        _listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
        _listener.Bind(new IPEndPoint(IPAddress.Loopback, port));
    }

    public async Task<ClientConnection> WaitForClient(Action<GameUpdate> onData, Action onClose)
    {
        _listener.Listen(100);
        return new ClientConnection(await _listener.AcceptAsync(), onData, onClose);
    }
}
