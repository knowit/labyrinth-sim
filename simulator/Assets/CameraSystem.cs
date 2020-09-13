using System.Collections;
using System.Collections.Generic;
using System.Net.Mqtt;
using System.Threading.Tasks;
using UnityEngine;

public class CameraSystem : ILabyrinthSystem
{
    private string _vrAddress = null;

    public int BasePort = 4049;

    private int _ballStatePort => BasePort;


    public OutboundChannel<BallState> BallStateChannel { get; private set; } = null;


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

                    mqttClient.PublishAsync(
                        new Mqtt.SystemOnline
                        {
                            address = this.GetLocalIPAddress()
                        }.AsMessage("labyrinth/camera/online"),
                        MqttQualityOfService.AtLeastOnce);

                    BallStateChannel = new OutboundChannel<BallState>(_vrAddress, _ballStatePort);
                }
                break;
        }
    }

    public void Update()
    {
        if (BallStateChannel != null)
        {
            _ = BallStateChannel.Send(new BallState
            {
                Position = new Vec2 { X = 0.0f, Y = 0.0f }
            });
        }
    }
}
