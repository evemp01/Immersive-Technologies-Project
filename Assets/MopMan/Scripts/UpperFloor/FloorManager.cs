using System.Collections.Generic;
using UnityEngine;

// Builds a (col,row) grid from tile world positions so the mop can clean a block around any tile.
public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance;

    [Tooltip("Parent of the GlassTile instances. Defaults to this object.")]
    public Transform tilesRoot;

    private readonly List<TileState> tiles = new List<TileState>();
    private readonly Dictionary<(int col, int row), TileState> grid =
        new Dictionary<(int, int), TileState>();

    private Vector3 origin;
    private float stepX = 1f;
    private float stepZ = 1f;

    void Awake()
    {
        Instance = this;
        if (tilesRoot == null) tilesRoot = transform;
    }

    void Start() => BuildGrid();

    private void BuildGrid()
    {
        tiles.Clear();
        grid.Clear();
        tilesRoot.GetComponentsInChildren(true, tiles);
        if (tiles.Count == 0)
        {
            Debug.LogWarning("FloorManager found no TileState children.", this);
            return;
        }

        // The grid step is the smallest gap between tile centres on each axis.
        var xs = new SortedSet<float>();
        var zs = new SortedSet<float>();
        float minX = float.MaxValue, minZ = float.MaxValue;
        foreach (var t in tiles)
        {
            Vector3 p = t.transform.position;
            xs.Add(Mathf.Round(p.x * 100f) / 100f);
            zs.Add(Mathf.Round(p.z * 100f) / 100f);
            if (p.x < minX) minX = p.x;
            if (p.z < minZ) minZ = p.z;
        }
        stepX = SmallestGap(xs);
        stepZ = SmallestGap(zs);
        origin = new Vector3(minX, 0f, minZ);

        foreach (var t in tiles)
        {
            Vector3 p = t.transform.position;
            t.col = Mathf.RoundToInt((p.x - origin.x) / stepX);
            t.row = Mathf.RoundToInt((p.z - origin.z) / stepZ);
            grid[(t.col, t.row)] = t;
        }
    }

    private static float SmallestGap(SortedSet<float> values)
    {
        float prev = float.NaN, gap = float.MaxValue;
        foreach (float v in values)
        {
            if (!float.IsNaN(prev))
            {
                float d = v - prev;
                if (d > 0.0001f && d < gap) gap = d;
            }
            prev = v;
        }
        return gap == float.MaxValue ? 1f : gap;
    }

    // Tile under a world position, or null if it falls outside the grid.
    public TileState GetTileAt(Vector3 worldPos)
    {
        int col = Mathf.RoundToInt((worldPos.x - origin.x) / stepX);
        int row = Mathf.RoundToInt((worldPos.z - origin.z) / stepZ);
        grid.TryGetValue((col, row), out TileState tile);
        return tile;
    }

    // Square block of tiles around `center` (range 1 = 1x1, 2 = 2x2, 3 = 3x3...).
    // Even sizes can't sit on a single tile, so they grow one extra tile toward +X/+Z.
    public void GetBlock(TileState center, int range, List<TileState> results)
    {
        results.Clear();
        if (center == null || range < 1) return;
        int lo = -((range - 1) / 2);
        int hi = range / 2;
        for (int dc = lo; dc <= hi; dc++)
        {
            for (int dr = lo; dr <= hi; dr++)
            {
                if (grid.TryGetValue((center.col + dc, center.row + dr), out TileState t))
                    results.Add(t);
            }
        }
    }
}
