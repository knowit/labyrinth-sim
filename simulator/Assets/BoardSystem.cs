using System.Net.Mqtt;
using System.Threading.Tasks;
using System;
using UnityEngine;

public class BoardSystem : ILabyrinthSystem
{
    private string _vrAddress = null;

    public int BasePort = 4049;

    private int _boardStatePort => BasePort + 1;
    private int _joystickStatePort => BasePort + 2;

    public OutboundChannel<BoardState> BoardStateChannel { get; private set; } = null;

    public InboundChannel<JoystickState> JoystickStateChannel { get; private set; } = null;


    public async Task SetupMqtt(IMqttClient mqttClient)
    {
        await mqttClient.SubscribeAsync(
            "labyrinth/vr/online", 
            MqttQualityOfService.AtLeastOnce);
    }

    // Start is called before the first frame update
    public async Task Start(IMqttClient mqttClient)
    {
        await mqttClient.PublishAsync(
            new Mqtt.SystemOnline { 
                    address = this.GetLocalIPAddress()
            }.AsMessage("labyrinth/board/online"),
            MqttQualityOfService.AtLeastOnce);

        JoystickStateChannel = new InboundChannel<JoystickState>(_joystickStatePort);
    }

    public void TopicReceived(string topic, Mqtt.MessageLoader message, IMqttClient mqttClient)
    {
        switch(topic)
        {
            case "labyrinth/vr/online":
                var msg = message.AsMessage<Mqtt.SystemOnline>();
                if(_vrAddress != msg.address)
                {
                    _vrAddress = msg.address;

                    Debug.Log($"[Board] Message from VR {_vrAddress}");

                    mqttClient.PublishAsync(
                        new Mqtt.SystemOnline
                        {
                            address = this.GetLocalIPAddress()
                        }.AsMessage("labyrinth/board/online"),
                        MqttQualityOfService.AtLeastOnce);

                    BoardStateChannel = new OutboundChannel<BoardState>(_vrAddress, _boardStatePort);
                }

                break;
        }
    }

    public void Update() 
    {
        if (BoardStateChannel != null)
        {
            BoardStateChannel.Send(new BoardState
            {
                Orientation = new Vec2 { 
                    X = UnityEngine.Random.Range(-1.0f, 1.0f), 
                    Y = UnityEngine.Random.Range(-1.0f, 1.0f)
                }
            });
        }
        
    }
}
