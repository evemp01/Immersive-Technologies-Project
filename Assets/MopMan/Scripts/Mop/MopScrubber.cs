using System.Collections.Generic;
using UnityEngine;

// Goes on the mop head (needs a trigger collider). Speed-gated: tiles only clean while the head is moving.
[RequireComponent(typeof(Collider))]
public class MopScrubber : MonoBehaviour
{
    [Tooltip("Mop type, sets the cleaned area and passes needed.")]
    public MopType mopType;

    [Tooltip("Minimum head speed (m/s) that counts as scrubbing.")]
    public float minScrubSpeed = 0.15f;

    [Tooltip("Seconds between passes on the same tile.")]
    public float passInterval = 0.15f;

    [Header("Debug")]
    public bool debug = false;

    private Vector3 lastPos;
    private float speed;
    private readonly List<TileState> block = new List<TileState>();
    private readonly Dictionary<TileState, float> nextPass = new Dictionary<TileState, float>();

    void Start() => lastPos = transform.position;

    void FixedUpdate()
    {
        speed = (transform.position - lastPos).magnitude / Mathf.Max(Time.fixedDeltaTime, 1e-5f);
        lastPos = transform.position;
    }

    void OnTriggerStay(Collider other)
    {
        if (mopType == null || FloorManager.Instance == null) return;
        if (speed < minScrubSpeed) return;

        TileState tile = other.GetComponentInParent<TileState>();
        if (tile == null) return;

        // Don't clean the same tile every frame it stays in contact.
        if (nextPass.TryGetValue(tile, out float t) && Time.time < t) return;
        nextPass[tile] = Time.time + passInterval;

        FloorManager.Instance.GetBlock(tile, mopType.tileRange, block);
        foreach (TileState b in block)
            b.ApplyCleanPass(mopType.neededPasses);

        if (debug) Debug.Log($"[MopScrubber] {mopType.displayName} scrubbed '{tile.name}'");
    }
}
