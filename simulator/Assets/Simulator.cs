using UnityEngine;
using UnityEngine.SceneManagement;

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
        Debug.Log("Waiting for connection");
        _connection = await SocketServer.WaitForClient(
            port: Port,
            update =>
            {
                if (update.Event == GameEvent.VrOrientation)
                {
                    Debug.Log("Received event 'VrOrientation'");
                    Board.MoveRotation(update.Data
                        .VrOrientationUpdate
                        .Orientation.ToUnityQuaternionAsEulerRotationXZ());
                }
            },
            () =>
            {
                // Reload scene on close connection
                Debug.Log("Connection closed");
                SceneManager.LoadScene(
                    SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
            });

        Debug.Log("Starting new game");
        _ballInstance = Instantiate(BallPrefab, SpawnLocation.position, SpawnLocation.rotation);

        Debug.Log("Sending 'Playing' event to client");
        await _connection.Send(new GameUpdate
        {
            Event = GameEvent.Playing
        });
    }

    async void LateUpdate()
    {
        if (_ballInstance != null && _connection != null)
        {
            await _connection.Send(new GameUpdate
            {
                Event = GameEvent.LabyrinthState,
                Data = new GameMessage
                {
                    LabyrinthStateUpdate = new LabyrinthStateUpdate
                    {
                        Position = _ballInstance.position.ToNormalizedCoordinate(),
                        BoardOrientation = Board.rotation.ToEulerRotationXZ()
                    }
                }
            });
        }
    }
}
