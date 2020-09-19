using System.Collections;
using System.Collections.Generic;
using System.Net.Mqtt;
using System.Threading.Tasks;
using UnityEngine;

public class CameraSystem : ILabyrinthSystem
{
    private string _vrAddress = null;

    private OutboundChannel<BallState> _ballStateChannel = null;


    public async Task SetupMqtt(IMqttClient mqttClient)
    {
        await mqttClient.SubscribeAsync(
            "labyrinth/vr/online", 
            MqttQualityOfService.AtLeastOnce);
    }

    public async Task Start(IMqttClient mqttClient)
    {
        await mqttClient.PublishAsync(
            new Mqtt.SystemOnline
            {
                address = this.GetLocalIPAddress()
            }.AsMessage("labyrinth/camera/online"),
            MqttQualityOfService.AtLeastOnce);
    }

    public void Start(string vrAddress)
    {
        _vrAddress = vrAddress;
        _ballStateChannel = new OutboundChannel<BallState>(
                _vrAddress, this.GetSimulator().BallStatePort);
    }

    public void TopicReceived(string topic, Mqtt.MessageLoader message, IMqttClient mqttClient)
    {
        switch (topic)
        {
            case "labyrinth/vr/online":
                var msg = message.AsMessage<Mqtt.SystemOnline>();

                if (_vrAddress != msg.address)
                {
                    _vrAddress = msg.address;
                    Debug.Log($"[Camera] Message from VR {msg.address}");

                    _ballStateChannel = new OutboundChannel<BallState>(
                        _vrAddress, this.GetSimulator().BallStatePort);
                }
                break;
        }
    }

    public void Update()
    {
    }
}
