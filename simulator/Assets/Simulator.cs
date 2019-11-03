using UnityEngine;

public class Simulator : MonoBehaviour
{
    public int Port = 11000;
    public Transform SpawnLocation;
    public Rigidbody BallPrefab;
    public Rigidbody Board;

    private Rigidbody _ballInstance;
    private ClientConnection _connection;

    async void Start()
    {
        _connection = await SocketServer.Open(Port, update =>
        {
            if (update.Event == GameEvent.VrOrientation)
            {
                var rot = update.Data
                    .VrOrientationUpdate
                    .Orientation.ToUnityQuaternionAsEulerRotationXZ();
                Board.MoveRotation(rot);
            }
        });

        _ballInstance = Instantiate(BallPrefab, SpawnLocation.position, SpawnLocation.rotation);
    }

    async void LateUpdate()
    {
        if (_ballInstance != null && _connection != null)
        {
            await _connection.Send(new GameUpdate
            {
                Event = GameEvent.LabyrinthState,
                Data = new LabyrinthStateUpdate
                {
                    Position = _ballInstance.position.ToNormalizedCoordinate(),
                    BoardOrientation = Board.rotation.ToEulerRotationXZ()
                }
            });
        }
    }
}
