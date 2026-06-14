using UnityEngine;
using UnityEngine.Events;
using Ubiq.Messaging;

// Win/restart for the two-player game. Each client reports only its own lobby
// presence; the win fires deterministically on both once both players are in
// the lobby with every key collected. Restart is one broadcast.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Reset targets")]
    public FloorManager floor;
    public KeyManager keys;
    public MopShelf mopShelf;

    [Tooltip("Lobby pads re-enabled on restart so players can re-enter the maze.")]
    public GameObject[] teleportPads;

    public UnityEvent OnWin;
    public UnityEvent OnRestart;

    private NetworkContext context;
    private bool localInLobby;
    private bool remoteInLobby;
    private bool won;

    void Awake() => Instance = this;
    void Start() => context = NetworkScene.Register(this);

    // Called by the lobby trigger for the local player only.
    public void SetLocalInLobby(bool inLobby)
    {
        if (localInLobby == inLobby) return;
        localInLobby = inLobby;
        context.SendJson(new Message { inLobby = inLobby });
        CheckWin();
    }

    // Called by the Play Again button.
    public void RequestRestart()
    {
        if (!won) return;
        context.SendJson(new Message { restart = true });
        DoRestart();
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        Message m = msg.FromJson<Message>();
        if (m.restart)
        {
            DoRestart();
        }
        else
        {
            remoteInLobby = m.inLobby;
            CheckWin();
        }
    }

    private void CheckWin()
    {
        if (won || keys == null || !keys.AllCollected) return;
        if (!localInLobby || !remoteInLobby) return;
        won = true;
        Debug.Log("[GameManager] You won! Both players in the lobby with all keys.");
        OnWin?.Invoke();
    }

    private void DoRestart()
    {
        won = false;
        if (floor != null) floor.ResetFloor();
        if (keys != null) keys.ResetKeys();
        if (mopShelf != null) mopShelf.ResetMop();
        foreach (GameObject pad in teleportPads)
            if (pad != null) pad.SetActive(true);
        OnRestart?.Invoke();
    }

    private struct Message
    {
        public bool restart;
        public bool inLobby;
    }
}
