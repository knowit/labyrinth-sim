using System.Net;
using System.Net.Mqtt;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;

public interface ILabyrinthSystem
{
    Task SetupMqtt(IMqttClient mqttClient);

    void TopicReceived(string topic, Mqtt.MessageLoader message, IMqttClient mqttClient);
    
    Task Start(IMqttClient mqttClient);

    void Start(string vrAddress);

    void Update();
}

public static class LabyrinthSystemExtension
{
    public static string GetLocalIPAddress(this ILabyrinthSystem _)
        => Dns.GetHostEntry(Dns.GetHostName())
            .AddressList
            .First(x => x.AddressFamily == AddressFamily.InterNetwork)
            .ToString();

    public static ISimulator GetSimulator(this ILabyrinthSystem _)
        => UnityEngine.Object.FindObjectOfType<SimulatorDirect>();
}