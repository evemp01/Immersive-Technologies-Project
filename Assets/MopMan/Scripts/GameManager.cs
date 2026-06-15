using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Ubiq.Messaging;
using Ubiq.Rooms;

// Win/lose/restart for the two-player game. Each client reports only its own
// lobby presence; the win fires deterministically on both once both players are
// in the lobby with every key collected. Game over is raised by the monster on
// each client (the hit event is already synced). Pad hides and restart are
// broadcast so both clients stay in sync.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Reset targets")]
    public FloorManager floor;
    public KeyManager keys;
    public MopShelf mopShelf;

    [Tooltip("Lobby pads, hidden on use and re-enabled on restart. Same order on both clients.")]
    public GameObject[] teleportPads;

    public UnityEvent OnWin;
    public UnityEvent OnGameOver;
    public UnityEvent OnRestart;

    private NetworkContext context;
    private RoomClient roomClient;
    private bool localInLobby;
    private bool remoteInLobby;
    private bool ended;

    void Awake() => Instance = this;

    void Start()
    {
        context = NetworkScene.Register(this);
        roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();
        if (roomClient != null)
        {
            roomClient.OnPeerAdded.AddListener(_ => ShowPadsIfReady());
            roomClient.OnJoinedRoom.AddListener(_ => ShowPadsIfReady());
        }

        // Pads stay hidden until a second player is in the room, so the first
        // player can't start (and hide a pad) before the other can hear it.
        for (int i = 0; i < teleportPads.Length; i++) SetPad(i, false);
        ShowPadsIfReady();
    }

    void Update()
    {
        // Debug shortcut: R actuates the Play Again button.
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            RequestRestart();
    }

    // Called by the lobby trigger for the local player only.
    public void SetLocalInLobby(bool inLobby)
    {
        if (localInLobby == inLobby) return;
        localInLobby = inLobby;
        context.SendJson(new Message { kind = Kind.Presence, inLobby = inLobby });
        CheckWin();
    }

    // Called by a start pad when the local player uses it.
    public void HidePad(GameObject pad)
    {
        int i = System.Array.IndexOf(teleportPads, pad);
        if (i < 0) { pad.SetActive(false); return; }
        SetPad(i, false);
        context.SendJson(new Message { kind = Kind.HidePad, padIndex = i });
    }

    // Called by the monster on each client when a player loses the last life.
    public void NotifyGameOver()
    {
        if (ended) return;
        ended = true;
        Debug.Log("[GameManager] Game over! A player lost all lives.");
        OnGameOver?.Invoke();
    }

    // Called by the Play Again button.
    public void RequestRestart()
    {
        if (!ended) return;
        context.SendJson(new Message { kind = Kind.Restart });
        DoRestart();
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        Message m = msg.FromJson<Message>();
        switch (m.kind)
        {
            case Kind.Restart: DoRestart(); break;
            case Kind.HidePad: SetPad(m.padIndex, false); break;
            default: remoteInLobby = m.inLobby; CheckWin(); break;
        }
    }

    private void CheckWin()
    {
        if (ended || keys == null || !keys.AllCollected) return;
        if (!localInLobby || !remoteInLobby) return;
        ended = true;
        Debug.Log("[GameManager] You won! Both players in the lobby with all keys.");
        OnWin?.Invoke();
    }

    private void DoRestart()
    {
        ended = false;
        if (floor != null) floor.ResetFloor();
        if (keys != null) keys.ResetKeys();
        if (mopShelf != null) mopShelf.ResetMop();
        ShowPadsIfReady();
        OnRestart?.Invoke();
    }

    // Reveals the start pads, but only once both players are connected.
    private void ShowPadsIfReady()
    {
        if (roomClient != null && !HasSecondPlayer()) return;
        for (int i = 0; i < teleportPads.Length; i++) SetPad(i, true);
    }

    private bool HasSecondPlayer()
    {
        foreach (var _ in roomClient.Peers) return true;
        return false;
    }

    private void SetPad(int index, bool active)
    {
        if (index >= 0 && index < teleportPads.Length && teleportPads[index] != null)
            teleportPads[index].SetActive(active);
    }

    private enum Kind { Presence, Restart, HidePad }

    private struct Message
    {
        public Kind kind;
        public bool inLobby;
        public int padIndex;
    }
}
