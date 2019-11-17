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

                try
                {
                    var bytes = _client.Receive(buffer, SocketFlags.None);
                    if (bytes > 0)
                    {
                        using (var ms = new MemoryStream(buffer))
                            onData(GameUpdate.Parser.ParseDelimitedFrom(ms));
                    }
                }
                catch (SocketException e)
                {
                    Debug.LogError(e);
                    if (_client.Connected)
                        _client.Shutdown(SocketShutdown.Both);
                    _onClose();
                }

                await Task.Yield();
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

        try
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
        catch (SocketException e) 
        {
            Debug.LogError(e);
            if (_client.Connected)
                _client.Shutdown(SocketShutdown.Both);
            _onClose();
        }
    }
};

public class SocketServer
{

    public async static Task<ClientConnection> WaitForClient(int port, Action<GameUpdate> onData, Action onClose)
    {
        var endpoint = new IPEndPoint(IPAddress.Loopback, port);
        var listener = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Debug.Log($"Bind socket {endpoint.Address}:{endpoint.Port}");
        
        listener.Bind(endpoint);
        listener.Listen(100);
        
        var connection = new ClientConnection(await listener.AcceptAsync(), onData, onClose);
        listener.Close();
        return connection;
    }
}
