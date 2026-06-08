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
    private int spent = 0;

    public int GetBalance() => collected - spent;

    void Awake() => Instance = this;
    void Start()
    {
        context = NetworkScene.Register(this);
        StartCoroutine(InitUI());
    }

    private IEnumerator InitUI()
    {
        yield return null;
        OnCounterUpdate?.Invoke($"{GetBalance()}");
    }

    public void Register(CoinItem coin) => coins[coin.id] = coin;

    public void Collect(string id, float respawnDelay = 0f, bool sendNetworkMessage = true)
    {
        if (coins.TryGetValue(id, out CoinItem coin) && coin.gameObject.activeSelf)
        {
            coin.gameObject.SetActive(false);
            collected++;

            OnCounterUpdate?.Invoke($"{GetBalance()}");

            if (sendNetworkMessage) context.SendJson(new Message { id = id });

            if (respawnDelay > 0f)
                StartCoroutine(RespawnAfterDelay(id, respawnDelay));
        }
    }

    // Returns false if balance is insufficient. Syncs the spend to the other client.
    public bool Spend(int amount, bool sendNetworkMessage = true)
    {
        if (GetBalance() < amount) return false;
        spent += amount;
        OnCounterUpdate?.Invoke($"{GetBalance()}");
        if (sendNetworkMessage) context.SendJson(new Message { spendAmount = amount });
        return true;
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
            if (sendNetworkMessage) context.SendJson(new Message { id = id, respawn = true });
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        Message m = msg.FromJson<Message>();
        if (m.spendAmount > 0)
            Spend(m.spendAmount, sendNetworkMessage: false);
        else if (m.respawn)
            Respawn(m.id, false);
        else
            Collect(m.id, sendNetworkMessage: false);
    }

    private struct Message
    {
        public string id;
        public bool respawn;
        public int spendAmount;
    }
}
