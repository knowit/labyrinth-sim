using System.Net.Mqtt;
using System.Threading.Tasks;
using System;
using UnityEngine;

public class BoardSystem : ILabyrinthSystem
{
    private string _vrAddress = null;

    private OutboundChannel<BoardState> _boardStateChannel = null;
    private InboundChannel<JoystickState> _joystickStateChannel  = null;


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

        _joystickStateChannel = new InboundChannel<JoystickState>(this.GetSimulator().JoystickStatePort);
    }

    public void Start (string vrAddress)
    {
        _vrAddress = vrAddress;
        _joystickStateChannel = new InboundChannel<JoystickState>(
            this.GetSimulator().JoystickStatePort);
        _boardStateChannel = new OutboundChannel<BoardState>(
            vrAddress, this.GetSimulator().BoardStatePort);
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

                    _boardStateChannel = new OutboundChannel<BoardState>(
                        _vrAddress, this.GetSimulator().BoardStatePort);
                }
                break;
        }
    }

    public void Update() 
    {
        var board = this.GetSimulator().Board;

        if (_boardStateChannel != null)
        {
            var rotation = board.rotation.ToEulerRotationXZ().WrapAngles();
            _boardStateChannel.Send(new BoardState
            {
                Orientation = rotation
            });
        }
        
        if(_joystickStateChannel != null && _joystickStateChannel.Message != null)
        {
            var lastMessage = _joystickStateChannel.Message;
            board.MoveRotation(Quaternion.RotateTowards(
                board.rotation,
                lastMessage.Orientation.ToQuaternion(),
                5.0f));
        }
    }
}
