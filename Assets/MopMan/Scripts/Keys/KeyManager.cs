using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Ubiq.Messaging;
using System.Collections;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance;
    
    public UnityEvent<string> OnCounterUpdate;
    public UnityEvent OnDoorUnlock;

    private NetworkContext context;
    private Dictionary<string, KeyItem> keys = new Dictionary<string, KeyItem>();
    private int collected = 0;

    void Awake() => Instance = this;
    void Start()
    {
        context = NetworkScene.Register(this);
        StartCoroutine(InitUI());
    }

    private IEnumerator InitUI()
    {
        yield return null; // wait one frame so all KeyItems finish their Start()
        OnCounterUpdate?.Invoke($"<voffset=0.3em><sprite name=\"key\"></voffset> {collected}/{keys.Count}");
    }
    public void Register(KeyItem key) => keys[key.id] = key;

    // Both local touches and network messages route through here
    public void Collect(string id, bool sendNetworkMessage = true)
    {
        if (keys.TryGetValue(id, out KeyItem key) && key.gameObject.activeSelf)
        {
            key.gameObject.SetActive(false);
            collected++;
            
            if (collected >= keys.Count) {OnDoorUnlock?.Invoke();
            OnCounterUpdate?.Invoke($"<voffset=0.3em><sprite name=\"key\"></voffset> {collected}/{keys.Count}");
            
            }
            else
            {
            OnCounterUpdate?.Invoke($"<voffset=0.3em><sprite name=\"key\"></voffset> {collected}/{keys.Count}");
                
            }

            if (sendNetworkMessage) context.SendJson(new Message { id = id });
        }
    }

    // Ubiq network receiver
    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg) 
        => Collect(msg.FromJson<Message>().id, false);

    private struct Message { public string id; }
}
