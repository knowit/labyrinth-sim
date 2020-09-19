using System.Collections.Generic;
using UnityEngine;

public class SimulatorDirect : MonoBehaviour, ISimulator
{
    public Transform SpawnLocation;
    public Rigidbody BallPrefab;
    public Rigidbody Board;

    Transform ISimulator.SpawnLocation => SpawnLocation;
    Rigidbody ISimulator.BallPrefab => BallPrefab;
    Rigidbody ISimulator.Board => Board;

    [Space]

    public string VrAddress = "127.0.0.1";

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

    void Start()
    {
        _systems.ForEach(s => s.Start(VrAddress));
    }

    void LateUpdate()
    {
        _systems.ForEach(s => s.Update());
    }
}
