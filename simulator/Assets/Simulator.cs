using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Mqtt;
using System;

public class Simulator : MonoBehaviour
{
    public string MqttHost = "127.0.0.1";
    public int MqttPort = 1883;

    //public Transform SpawnLocation;
    //public Rigidbody BallPrefab;
    //public Rigidbody Board;

    //private Rigidbody _ballInstance;

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
