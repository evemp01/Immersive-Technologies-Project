using UnityEngine;

// One per GlassTile. Tracks dirtiness (0–1) and updates colour via MaterialPropertyBlock.
[RequireComponent(typeof(Renderer))]
public class TileState : MonoBehaviour
{
    [Range(0f, 1f)] public float dirtiness = 0f;

    [Header("Appearance")]
    [Tooltip("Colour when fully clean (the Glass material default, #00CAFF2B).")]
    public Color cleanColor = new Color(0f, 0.7921569f, 1f, 0.16862746f);

    [Tooltip("Colour when fully dirty.")]
    public Color dirtyColor = new Color(0.33f, 0.28f, 0.19f, 0.92f);

    [Header("Cleaning")]
    [Tooltip("Grace time after a mop pass before the tile can get dirty again.")]
    public float cleanCooldown = 1f;

    // Grid coordinates, filled in by FloorManager.
    [HideInInspector] public int col = -1;
    [HideInInspector] public int row = -1;

    private Renderer rend;
    private MaterialPropertyBlock mpb;
    private float dirtBlockedUntil;
    private float cleanBlockedUntil;
    private bool firstPassDone = false;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public bool IsClean => dirtiness <= 0.001f;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
        ApplyVisual();
    }

    public void AddDirt(float amount, float cap = 1f)
    {
        if (Time.time < dirtBlockedUntil) return;
        if (dirtiness >= cap) return;
        bool wasClean = IsClean;
        SetDirtiness(Mathf.Min(dirtiness + amount, cap));
        if (wasClean && !IsClean) firstPassDone = false;
    }

    // First pass always removes 25%; subsequent passes remove 1/neededPasses each.
    // The tile blocks re-cleaning for cleanCooldown seconds, forcing real back-and-forth.
    public void ApplyCleanPass(int neededPasses)
    {
        if (Time.time < cleanBlockedUntil) return;
        if (neededPasses < 1) neededPasses = 1;
        float removal = (neededPasses > 1 && !firstPassDone) ? 0.25f : 1f / neededPasses;
        firstPassDone = true;
        SetDirtiness(dirtiness - removal);
        if (IsClean) firstPassDone = false;
        dirtBlockedUntil = Time.time + cleanCooldown;
        cleanBlockedUntil = Time.time + cleanCooldown;
    }

    public void SetDirtiness(float value)
    {
        dirtiness = Mathf.Clamp01(value);
        ApplyVisual();
    }

    private void ApplyVisual()
    {
        if (rend == null) return;
        rend.GetPropertyBlock(mpb);
        mpb.SetColor(BaseColorId, Color.Lerp(cleanColor, dirtyColor, dirtiness));
        rend.SetPropertyBlock(mpb);
    }
}
