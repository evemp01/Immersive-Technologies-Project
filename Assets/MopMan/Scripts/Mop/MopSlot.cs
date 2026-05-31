using UnityEngine;

// A single mop on the shelf. Needs a trigger collider: when the player is inside it and presses
// the action key, the slot buys the mop (first time) and equips it. Locked at start unless
// startsUnlocked is on.
[RequireComponent(typeof(Collider))]
public class MopSlot : MonoBehaviour
{
    [Tooltip("Mop this slot sells.")]
    public MopType mopType;

    [Tooltip("Already owned at start (use this for the base mop).")]
    public bool startsUnlocked = false;

    [Header("Visuals (optional)")]
    [Tooltip("Shown while locked, hidden once bought.")]
    public GameObject lockedVisual;

    [Tooltip("Renderer tinted by mop colour / lock state.")]
    public Renderer tintRenderer;
    public Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color unlockedColor = Color.white;

    [Header("Debug")]
    public bool debug = false;

    public bool Unlocked { get; private set; }

    private MopShelf shelf;

    void Awake()
    {
        shelf = GetComponentInParent<MopShelf>();
        Unlocked = startsUnlocked;
        if (Unlocked && shelf != null) shelf.MarkUnlocked(mopType);
        UpdateVisual();
    }

    // Called by MopController when the player presses the action key inside this slot.
    public void Interact(MopController controller)
    {
        if (mopType == null) return;

        if (!Unlocked)
        {
            bool bought = shelf != null && shelf.TryPurchase(mopType);
            if (!bought)
            {
                if (debug) Debug.Log($"[MopSlot] can't buy {mopType.displayName} yet");
                return;
            }
            Unlocked = true;
            UpdateVisual();
        }

        // Owned: equip it, so the level's mop changes to this type.
        if (shelf != null)
        {
            shelf.Equip(mopType);
            shelf.NotifyEquipped(this);
        }
    }

    private void UpdateVisual()
    {
        if (lockedVisual != null) lockedVisual.SetActive(!Unlocked);
        if (tintRenderer == null) return;
        // Owned mops show their head colour; locked ones are dimmed toward lockedColor.
        Color typeColor = mopType != null ? mopType.headColor : unlockedColor;
        tintRenderer.material.color = Unlocked ? typeColor : Color.Lerp(typeColor, lockedColor, 0.7f);
    }

    void OnTriggerEnter(Collider other)
    {
        MopController mc = other.GetComponentInParent<MopController>();
        if (mc != null) mc.currentSlot = this;
    }

    void OnTriggerExit(Collider other)
    {
        MopController mc = other.GetComponentInParent<MopController>();
        if (mc != null && mc.currentSlot == this) mc.currentSlot = null;
    }
}
