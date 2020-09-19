using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Mqtt;
using System;

public class SimulatorMqtt : MonoBehaviour, ISimulator
{
    public string MqttHost = "127.0.0.1";
    public int MqttPort = 1883;

    [Space]

    public Transform SpawnLocation;
    public Rigidbody BallPrefab;
    public Rigidbody Board;

    Transform ISimulator.SpawnLocation => SpawnLocation;
    Rigidbody ISimulator.BallPrefab => BallPrefab;
    Rigidbody ISimulator.Board => Board;

    [Space]

    public int BoardStatePort = 4049;
    public int JoystickStatePort = 4050;
    public int BallStatePort = 4051;

    int ISimulator.BoardStatePort => BoardStatePort;
    int ISimulator.JoystickStatePort => JoystickStatePort;
    int ISimulator.BallStatePort => BallStatePort;

    private List<ILabyrinthSystem> _systems = new List<ILabyrinthSystem>()
    {
        new BoardSystem(),
        new CameraSystem()
    };

    async void Start()
    {
        await Task.WhenAll(_systems.Select(async system => {
            var mqttClient = await MqttClient.CreateAsync(MqttHost, MqttPort);
            mqttClient.MessageStream.Subscribe(message =>
                system.TopicReceived(
                    message.Topic, 
                    new Mqtt.MessageLoader { raw = message.Payload }, mqttClient));

            var session = await mqttClient.ConnectAsync();
            if (session == SessionState.CleanSession)
            {
               await system.SetupMqtt(mqttClient);
            }

            return system.Start(mqttClient);
        }));
    }

    void LateUpdate()
    {
        _systems.ForEach(x => x.Update());
    }
}
