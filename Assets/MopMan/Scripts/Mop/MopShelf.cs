using UnityEngine;

// Owns the single mop in the level: swaps its type on purchase/equip,
// enforces sequential unlock, and delegates the coin balance to CoinManager.
public class MopShelf : MonoBehaviour
{
    [Header("The mop in the level")]
    [Tooltip("Root GameObject of the mop — hidden at start, shown on first purchase.")]
    public GameObject sceneMopRoot;

    [Tooltip("MopScrubber on the mop's head. Equipping swaps its type.")]
    public MopScrubber sceneMop;

    [Tooltip("Optional mop head Renderer, tinted to the equipped mop's colour.")]
    public Renderer sceneMopHeadRenderer;

    [Tooltip("Last equipped slot, for reference / a future HUD.")]
    public MopSlot equippedSlot;

    // Highest upgradeIndex owned so far. -1 = nothing yet; startsUnlocked slots raise it.
    public int HighestUnlockedIndex { get; private set; } = -1;

    void Start()
    {
        if (sceneMopRoot != null)
            sceneMopRoot.SetActive(false);
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

    public bool TryPurchase(MopType mop)
    {
        if (!IsNextInSequence(mop)) return false;
        if (!CanAfford(mop)) return false;
        Spend(mop.price);
        MarkUnlocked(mop);
        return true;
    }

    public void Equip(MopType mop)
    {
        if (sceneMopRoot != null) sceneMopRoot.SetActive(true);
        if (sceneMop != null) sceneMop.mopType = mop;
        if (sceneMopHeadRenderer != null && mop != null)
            sceneMopHeadRenderer.material.color = mop.headColor;
    }

    public void NotifyEquipped(MopSlot slot) => equippedSlot = slot;
}
