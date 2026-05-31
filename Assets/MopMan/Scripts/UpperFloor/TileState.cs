using UnityEngine;

// One per GlassTile. Holds how dirty the tile is (0 = clean, 1 = fully dirty) and updates its
// colour through a MaterialPropertyBlock, so every tile can share the single Glass material.
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
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public bool IsClean => dirtiness <= 0.001f;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
        ApplyVisual();
    }

    public void AddDirt(float amount)
    {
        if (Time.time < dirtBlockedUntil) return; // still cooling down after a clean
        SetDirtiness(dirtiness + amount);
    }

    // One mop pass removes 1/neededPasses of the grime, then blocks dirt for a short while.
    public void ApplyCleanPass(int neededPasses)
    {
        if (neededPasses < 1) neededPasses = 1;
        SetDirtiness(dirtiness - 1f / neededPasses);
        dirtBlockedUntil = Time.time + cleanCooldown;
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
