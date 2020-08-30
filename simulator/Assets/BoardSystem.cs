using System.Net.Mqtt;
using System.Threading.Tasks;
using System;
using UnityEngine;

public class BoardSystem : ILabyrinthSystem
{
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
    }

    public void TopicReceived(string topic, Mqtt.MessageLoader message, IMqttClient mqttClient)
    {
        switch(topic)
        {
            case "labyrinth/vr/online":
                var msg = message.AsMessage<Mqtt.SystemOnline>();
                Debug.Log($"[Board] Message from VR {msg.address}");

                mqttClient.PublishAsync(
                    new Mqtt.SystemOnline
                    {
                        address = this.GetLocalIPAddress()
                    }.AsMessage("labyrinth/board/online"),
                    MqttQualityOfService.AtLeastOnce);
                break;
        }
    }

    public void Update() { }
}
