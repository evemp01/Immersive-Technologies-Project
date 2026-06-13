using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MopSlot : MonoBehaviour
{
    [Tooltip("Mop this slot sells.")]
    public MopType mopType;

    [Tooltip("Already owned at start (use this for the base mop).")]
    public bool startsUnlocked = false;

    [Header("Visuals (optional)")]
    [Tooltip("Renderer tinted by mop colour / lock state.")]
    public Renderer tintRenderer;
    public Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color unlockedColor = Color.white;

    [Tooltip("TMP label on the sign — updated automatically with name, specs, price and status.")]
    public TMP_Text label;

    [Header("Error flash")]
    public Color errorColor = Color.red;
    public float flashDuration = 2f;

    public bool Unlocked { get; private set; }

    private MopShelf shelf;
    private Coroutine flashRoutine;

    void Awake()
    {
        shelf = GetComponentInParent<MopShelf>();
        Unlocked = startsUnlocked;
        if (Unlocked && shelf != null) shelf.MarkUnlocked(mopType);
        UpdateVisual();
    }

    // Called from XRSimpleInteractable.selectEntered in the Inspector (VR path).
    public void InteractVR() => Interact(null);

    public void Interact(MopController controller = null)
    {
        if (mopType == null) return;

        if (!Unlocked)
        {
            if (shelf == null) return;

            if (!shelf.IsNextInSequence(mopType))
            {
                FlashError("Unlock the\nprevious mop first!");
                return;
            }
            if (!shelf.CanAfford(mopType))
            {
                FlashError("Not enough coins!");
                return;
            }

            shelf.TryPurchase(mopType);
            Unlocked = true;
            UpdateVisual();
        }

        if (shelf != null)
        {
            shelf.Equip(mopType);
            shelf.NotifyEquipped(this);
        }
    }

    private void FlashError(string message)
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(ErrorFlash(message));
    }

    private IEnumerator ErrorFlash(string message)
    {
        string originalText = label != null ? label.text : "";
        Color baseColor = tintRenderer != null ? tintRenderer.material.color : errorColor;

        if (label != null) label.text = message;
        if (tintRenderer != null) tintRenderer.material.color = errorColor;

        yield return new WaitForSeconds(flashDuration);

        if (tintRenderer != null) tintRenderer.material.color = baseColor;
        if (label != null) label.text = originalText;
        flashRoutine = null;
    }

    private void UpdateVisual()
    {
        if (tintRenderer != null)
        {
            Color typeColor = mopType != null ? mopType.headColor : unlockedColor;
            tintRenderer.material.color = Unlocked ? typeColor : Color.Lerp(typeColor, lockedColor, 0.7f);
        }

        if (label != null && mopType != null)
        {
            string size = $"{mopType.tileRange}x{mopType.tileRange}";
            string passes = mopType.neededPasses == 1 ? "1 pass" : $"{mopType.neededPasses} passes";
            string price = mopType.price == 0 ? "Free" : $"{mopType.price} coins";
            string status = Unlocked ? "[ Unlocked ]" : "[ Locked ]";
            label.text = $"{mopType.displayName}\n{size} / {passes}\n{price}\n{status}";
        }
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
