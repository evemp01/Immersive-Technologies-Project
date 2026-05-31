using UnityEngine;

// The shelf logic: owns the single mop in the level, swaps its type when the player picks one
// (upgrade or downgrade), and keeps unlocking sequential (mop N needs mop N-1 first). The coin
// system isn't done yet, so for now a placeholder wallet affords everything.
public class MopShelf : MonoBehaviour
{
    [Header("The mop in the level")]
    [Tooltip("MopScrubber on the mop's head. Equipping swaps its type.")]
    public MopScrubber sceneMop;

    [Tooltip("Optional mop head Renderer, tinted to the equipped mop's colour.")]
    public Renderer sceneMopHeadRenderer;

    [Header("Money (placeholder until the coin system is in)")]
    public int playerMoney = 9999;

    [Tooltip("Last equipped slot, for reference / a future HUD.")]
    public MopSlot equippedSlot;

    // Highest upgradeIndex owned so far. -1 = nothing yet; startsUnlocked slots raise it.
    public int HighestUnlockedIndex { get; private set; } = -1;

    void Start()
    {
        // Apply the mop that's already loaded, so its colour shows before any pick.
        if (sceneMop != null && sceneMop.mopType != null)
            Equip(sceneMop.mopType);
    }

    // --- Coin hooks: the teammate's system replaces these two ---
    public bool CanAfford(MopType mop) => mop != null && playerMoney >= mop.price;

    private void Spend(int amount) => playerMoney -= amount;
    // ------------------------------------------------------------

    public bool IsNextInSequence(MopType mop) =>
        mop != null && mop.upgradeIndex <= HighestUnlockedIndex + 1;

    public void MarkUnlocked(MopType mop)
    {
        if (mop != null && mop.upgradeIndex > HighestUnlockedIndex)
            HighestUnlockedIndex = mop.upgradeIndex;
    }

    // Buy a mop: it has to be the next one in the chain and affordable.
    public bool TryPurchase(MopType mop)
    {
        if (!IsNextInSequence(mop)) return false;
        if (!CanAfford(mop)) return false;
        Spend(mop.price);
        MarkUnlocked(mop);
        return true;
    }

    // Turn the level's mop into this type.
    public void Equip(MopType mop)
    {
        if (sceneMop != null) sceneMop.mopType = mop;
        if (sceneMopHeadRenderer != null && mop != null)
            sceneMopHeadRenderer.material.color = mop.headColor;
    }

    public void NotifyEquipped(MopSlot slot) => equippedSlot = slot;
}
