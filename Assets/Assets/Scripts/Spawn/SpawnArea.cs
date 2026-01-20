using UnityEngine;

[AddComponentMenu("Spawn/SpawnArea")]
public class SpawnArea : MonoBehaviour
{
    public enum Mode { Corner, Random, Grid }
    public Mode mode = Mode.Corner;

    [Tooltip("If using a BoxCollider, SpawnArea will use its bounds. Otherwise it will use the transform scale as a box.")]
    public bool useColliderIfAvailable = true;

    [Tooltip("Offset from the computed spawn point in local space")]
    public Vector3 localOffset = new Vector3(0f, 0.5f, 0f);

    // Grid settings (unused for Corner)
    public Vector2Int gridSize = new Vector2Int(2, 2);
    public float gridPadding = 0.5f;

    /// <summary>
    /// Returns a spawn position in world space based on the selected mode and an index.
    /// For Mode.Corner the index is ignored and the corner of the area (min local corner) is used.
    /// </summary>
    public Vector3 GetSpawnPosition(int index = 0)
    {
        Bounds bounds = GetBounds();

        switch (mode)
        {
            case Mode.Random:
                {
                    var x = Random.Range(bounds.min.x, bounds.max.x);
                    var z = Random.Range(bounds.min.z, bounds.max.z);
                    var y = bounds.max.y + localOffset.y;
                    var pos = new Vector3(x, y, z) + new Vector3(localOffset.x, 0f, localOffset.z);
                    LogIfSuspicious(pos);
                    return pos;
                }
            case Mode.Grid:
                {
                    int cols = Mathf.Max(1, gridSize.x);
                    int rows = Mathf.Max(1, gridSize.y);
                    int idx = Mathf.Clamp(index, 0, cols * rows - 1);
                    int col = idx % cols;
                    int row = idx / cols;
                    float sx = Mathf.Lerp(bounds.min.x, bounds.max.x, cols == 1 ? 0.5f : (col / (float)(cols - 1)));
                    float sz = Mathf.Lerp(bounds.min.z, bounds.max.z, rows == 1 ? 0.5f : (row / (float)(rows - 1)));
                    var pos = new Vector3(sx, bounds.max.y + localOffset.y, sz) + new Vector3(localOffset.x, 0f, localOffset.z);
                    LogIfSuspicious(pos);
                    return pos;
                }
            case Mode.Corner:
            default:
                // Use world-space bounds.min (corner) and apply simple world offset.
                var corner = bounds.min + new Vector3(localOffset.x, localOffset.y, localOffset.z);
                LogIfSuspicious(corner);
                return corner;
        }
    }

    Bounds GetBounds()
    {
        var col = GetComponent<BoxCollider>();
        if (useColliderIfAvailable && col != null)
        {
            // Use collider.bounds which is already in world space and accounts for lossyScale
            return col.bounds;
        }

        // fallback: build a bounds centered on transform.position using lossyScale as size
        Vector3 worldCenter2 = transform.position;
        Vector3 worldSize2 = transform.lossyScale;
        worldSize2.x = Mathf.Max(1f, worldSize2.x);
        worldSize2.z = Mathf.Max(1f, worldSize2.z);
        worldSize2.y = Mathf.Max(1f, worldSize2.y);
        return new Bounds(worldCenter2, worldSize2);
    }

    void LogIfSuspicious(Vector3 pos)
    {
        if (pos.sqrMagnitude > 1000000f) // > 1000 units away squared
        {
            Debug.LogWarning($"SpawnArea produced a very large spawn position {pos}. Check SpawnArea bounds/scale and localOffset.", this);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        var bounds = GetBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        // draw spawn point for index 0
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(GetSpawnPosition(0), 0.15f);
    }
}
