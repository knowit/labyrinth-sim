using UnityEngine;

public interface ISimulator
{
    Transform SpawnLocation { get; }
    Rigidbody BallPrefab { get; }
    Rigidbody Board { get; }

    int BoardStatePort { get; }
    int JoystickStatePort { get; }
    int BallStatePort { get; }
}
