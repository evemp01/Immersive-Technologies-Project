using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.Events;

// Owns the single mop in the level: swaps its type on purchase/equip,
// enforces sequential unlock, and delegates the coin balance to CoinManager.
public class MopShelf : MonoBehaviour
{
    [Header("The mop in the level")]

    [Header("HUD")]
    public MopCounterUI mopCounterUI;

    [Tooltip("Root GameObject of the mop — hidden at start, shown on first purchase.")]
    public GameObject sceneMopRoot;

    [Tooltip("MopScrubber on the mop's head. Equipping swaps its type.")]
    public MopScrubber sceneMop;

    [Tooltip("Optional mop head Renderer, tinted to the equipped mop's colour.")]
    public Renderer sceneMopHeadRenderer;

    [Tooltip("Last equipped slot, for reference / a future HUD.")]
    public MopSlot equippedSlot;

    [Tooltip("Fired on both clients whenever the equipped mop changes.")]
    public UnityEvent<MopType> OnMopEquipped;

    // Highest upgradeIndex owned so far. -1 = nothing yet; startsUnlocked slots raise it.
    public int HighestUnlockedIndex { get; private set; } = -1;

    private NetworkContext context;
    private MopSlot[] slots;

    private struct Message { public int upgradeIndex; }

    void Start()
    {
        context = NetworkScene.Register(this);
        slots = GetComponentsInChildren<MopSlot>();
        if (sceneMopRoot != null)
            sceneMopRoot.SetActive(false);
        
        mopCounterUI?.UpdateMop(null);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        int index = msg.FromJson<Message>().upgradeIndex;
        foreach (var slot in slots)
            if (slot.mopType != null && slot.mopType.upgradeIndex == index)
            {
                // Equipping a mop means it (and everything below it) is unlocked
                // for this client too, so both players share the same shop state.
                MarkUnlocked(slot.mopType);
                RefreshSlotLocks();
                Equip(slot.mopType, sendNetworkMessage: false);
                return;
            }
    }

    public bool CanAfford(MopType mop) =>
        mop != null && CoinManager.Instance != null && CoinManager.Instance.GetBalance() >= mop.price;

    private void Spend(int amount) => CoinManager.Instance?.Spend(amount);

    public bool IsNextInSequence(MopType mop) =>
        mop != null && mop.upgradeIndex <= HighestUnlockedIndex + 1;

    public void MarkUnlocked(MopType mop)
    {
        if (mop != null && mop.upgradeIndex > HighestUnlockedIndex)
            HighestUnlockedIndex = mop.upgradeIndex;
    }

    private void RefreshSlotLocks()
    {
        foreach (MopSlot slot in slots)
            if (slot.mopType != null)
                slot.SetUnlocked(slot.mopType.upgradeIndex <= HighestUnlockedIndex);
    }

    public bool TryPurchase(MopType mop)
    {
        if (!IsNextInSequence(mop)) return false;
        if (!CanAfford(mop)) return false;
        Spend(mop.price);
        MarkUnlocked(mop);
        return true;
    }

    public void Equip(MopType mop, bool sendNetworkMessage = true)
    {
        if (sceneMopRoot != null) sceneMopRoot.SetActive(true);
        if (sceneMop != null) sceneMop.mopType = mop;
        if (sceneMopHeadRenderer != null && mop != null)
            sceneMopHeadRenderer.material.color = mop.headColor;

        if (mopCounterUI == null)
            mopCounterUI = FindObjectOfType<MopCounterUI>();
        mopCounterUI?.UpdateMop(mop);
        OnMopEquipped?.Invoke(mop);
        if (sendNetworkMessage && mop != null)
            context.SendJson(new Message { upgradeIndex = mop.upgradeIndex });
    }

    public void NotifyEquipped(MopSlot slot) => equippedSlot = slot;

    // Unlocks persist across games. Reset only un-equips: the mop disappears,
    // and clicking an already-unlocked slot re-equips it for free.
    public void ResetMop()
    {
        equippedSlot = null;
        if (sceneMopRoot != null) sceneMopRoot.SetActive(false);
        OnMopEquipped?.Invoke(null);
    }
}
