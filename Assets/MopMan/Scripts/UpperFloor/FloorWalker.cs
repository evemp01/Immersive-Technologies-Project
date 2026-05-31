using System.Collections.Generic;
using UnityEngine;

// Sits on the upper player. While they walk, a downward ray finds the tile below and dirties
// a block around it: the centre tile fastest, the ones around it slower and each at its own
// speed, so the grime spreads unevenly. Cleaning is the mop's job, not this.
public class FloorWalker : MonoBehaviour
{
    [Tooltip("Grime per second added to the centre tile while walking.")]
    public float dirtPerSecond = 1f;

    [Tooltip("Size of the dirtied block. 3 = 3x3.")]
    public int dirtRange = 3;

    [Tooltip("Outer tiles dirty at a random fraction of the centre rate, within this range.")]
    [Range(0f, 1f)] public float edgeMinFactor = 0.1f;
    [Range(0f, 1f)] public float edgeMaxFactor = 0.4f;

    [Tooltip("Only dirty while the player is moving.")]
    public bool requireMovement = true;

    [Tooltip("Speed (m/s) above which the player counts as walking.")]
    public float moveThreshold = 0.05f;

    [Tooltip("Layer the floor tiles are on (UpperFloor).")]
    public LayerMask floorMask = ~0;

    public float rayLength = 5f;
    public float rayStartHeight = 1f;

    [Tooltip("Cast from here. Empty = this object; for VR drag the head / Main Camera.")]
    public Transform raySource;

    [Header("Debug")]
    public bool debug = false;

    private Vector3 lastPos;
    private readonly List<TileState> block = new List<TileState>();

    private Transform Source => raySource != null ? raySource : transform;

    void Start() => lastPos = Source.position;

    void Update()
    {
        Vector3 pos = Source.position;
        float planarSpeed = new Vector2(pos.x - lastPos.x, pos.z - lastPos.z).magnitude
                            / Mathf.Max(Time.deltaTime, 1e-5f);
        lastPos = pos;

        if (requireMovement && planarSpeed < moveThreshold) return;

        Vector3 rayOrigin = pos + Vector3.up * rayStartHeight;
        float dist = rayLength + rayStartHeight;
        if (debug) Debug.DrawRay(rayOrigin, Vector3.down * dist, Color.red, 0.5f);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, dist, floorMask, QueryTriggerInteraction.Ignore))
        {
            TileState center = hit.collider.GetComponentInParent<TileState>();
            if (center != null)
                DirtyBlock(center, Time.deltaTime);
            else if (debug)
                Debug.Log($"[FloorWalker] hit '{hit.collider.name}' but it has no TileState");
        }
        else if (debug)
        {
            Debug.Log("[FloorWalker] ray hit nothing (check floorMask / rayLength / height)");
        }
    }

    private void DirtyBlock(TileState center, float dt)
    {
        if (FloorManager.Instance == null)
        {
            center.AddDirt(dirtPerSecond * dt); // no grid yet, just dirty the centre
            return;
        }

        FloorManager.Instance.GetBlock(center, dirtRange, block);
        foreach (TileState t in block)
        {
            float factor = t == center
                ? 1f
                : Mathf.Lerp(edgeMinFactor, edgeMaxFactor, Hash01(t.col, t.row));
            t.AddDirt(dirtPerSecond * factor * dt);
        }
    }

    // Fixed per-tile value from its grid coords, so each tile keeps the same dirtying speed
    // instead of flickering every frame.
    private static float Hash01(int c, int r)
    {
        unchecked
        {
            uint h = (uint)(c * 73856093) ^ (uint)(r * 19349663);
            return (h % 1000u) / 1000f;
        }
    }
}
