using System.Collections;
using System.Collections.Generic;
using System.Net.Mqtt;
using System.Threading.Tasks;
using UnityEngine;

public class CameraSystem : ILabyrinthSystem
{
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
                // TODO Start or restart UDP broadcaster
                Debug.Log($"[Camera] Message from VR {msg.address}");

                mqttClient.PublishAsync(
                    new Mqtt.SystemOnline
                    {
                        address = this.GetLocalIPAddress()
                    }.AsMessage("labyrinth/camera/online"),
                    MqttQualityOfService.AtLeastOnce);
                break;
        }
    }

    public void Update()
    {
    }
}
