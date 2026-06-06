using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Ubiq.Messaging;
using System.Collections;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public UnityEvent<string> OnCounterUpdate;

    private NetworkContext context;
    private Dictionary<string, CoinItem> coins = new Dictionary<string, CoinItem>();
    private int collected = 0;

    void Awake() => Instance = this;
    void Start()
    {
        context = NetworkScene.Register(this);
        StartCoroutine(InitUI());
    }

    private IEnumerator InitUI()
    {
        yield return null; // wait one frame so all CoinItems finish their Start()
        OnCounterUpdate?.Invoke($"{collected}");
    }

    public void Register(CoinItem coin) => coins[coin.id] = coin;

    // Returns how many coins have been collected so far
    public int GetTotalCollected() => collected;

    // Both local touches and network messages route through here
    // CoinManager is always active, so it safely owns the respawn coroutine
    public void Collect(string id, float respawnDelay = 0f, bool sendNetworkMessage = true)
    {
        if (coins.TryGetValue(id, out CoinItem coin) && coin.gameObject.activeSelf)
        {
            coin.gameObject.SetActive(false);
            collected++;

            OnCounterUpdate?.Invoke($"{collected}");

            if (sendNetworkMessage) context.SendJson(new Message { id = id, respawn = false });

            if (respawnDelay > 0f)
                StartCoroutine(RespawnAfterDelay(id, respawnDelay));
        }
    }

    private IEnumerator RespawnAfterDelay(string id, float delay)
    {
        yield return new WaitForSeconds(delay);
        Respawn(id);
    }

    public void Respawn(string id, bool sendNetworkMessage = true)
    {
        if (coins.TryGetValue(id, out CoinItem coin) && !coin.gameObject.activeSelf)
        {
            coin.gameObject.SetActive(true);

            OnCounterUpdate?.Invoke($"{collected}");

            if (sendNetworkMessage) context.SendJson(new Message { id = id, respawn = true });
        }
    }

    // Ubiq network receiver
    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        Message m = msg.FromJson<Message>();
        if (m.respawn)
            Respawn(m.id, false);
        else
            Collect(m.id, sendNetworkMessage: false);
    }

    private struct Message
    {
        public string id;
        public bool respawn;
    }
}
