using System.Collections.Generic;
using UnityEngine;

// Sits on the upper player. While they walk, a downward ray finds the tile below and dirties
// a 5x5 block: centre fastest, inner ring (distance 1) medium, outer ring (distance 2) slowest.
public class FloorWalker : MonoBehaviour
{
    [Tooltip("Grime per second added to the centre tile while walking.")]
    public float dirtPerSecond = 1f;

    [Header("Ring factors (fraction of dirtPerSecond)")]
    [Range(0f, 1f)] public float innerFactor = 0.5f;
    [Range(0f, 1f)] public float outerMinFactor = 0.05f;
    [Range(0f, 1f)] public float outerMaxFactor = 0.2f;

    [Tooltip("Dirt rate (fraction of dirtPerSecond) and max dirtiness while the player stands still.")]
    public float stillDirtFactor = 0.3f;
    [Range(0f, 1f)] public float stillMaxDirtiness = 0.85f;

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

        bool moving = planarSpeed >= moveThreshold;
        float rate = moving ? dirtPerSecond : dirtPerSecond * stillDirtFactor;
        float maxDirt = moving ? 1f : stillMaxDirtiness;

        Vector3 rayOrigin = pos + Vector3.up * rayStartHeight;
        float dist = rayLength + rayStartHeight;
        if (debug) Debug.DrawRay(rayOrigin, Vector3.down * dist, Color.red, 0.5f);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, dist, floorMask, QueryTriggerInteraction.Ignore))
        {
            TileState center = hit.collider.GetComponentInParent<TileState>();
            if (center != null)
                DirtyBlock(center, Time.deltaTime, rate, maxDirt);
            else if (debug)
                Debug.Log($"[FloorWalker] hit '{hit.collider.name}' but it has no TileState");
        }
        else if (debug)
            Debug.Log("[FloorWalker] ray hit nothing (check floorMask / rayLength / height)");
    }

    private void DirtyBlock(TileState center, float dt, float rate, float maxDirt)
    {
        if (FloorManager.Instance == null)
        {
            center.AddDirt(rate * dt, maxDirt);
            return;
        }

        FloorManager.Instance.GetBlock(center, 5, block);
        foreach (TileState t in block)
        {
            int dist = Mathf.Max(Mathf.Abs(t.col - center.col), Mathf.Abs(t.row - center.row));
            float factor = dist == 0 ? 1f
                         : dist == 1 ? innerFactor
                         : Mathf.Lerp(outerMinFactor, outerMaxFactor, Hash01(t.col, t.row));
            t.AddDirt(rate * factor * dt, maxDirt);
        }
    }

    private static float Hash01(int c, int r)
    {
        unchecked
        {
            uint h = (uint)(c * 73856093) ^ (uint)(r * 19349663);
            return (h % 1000u) / 1000f;
        }
    }
}
