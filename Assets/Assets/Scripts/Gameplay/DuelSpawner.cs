using UnityEngine;
#if UNITY_NETCODE
using Unity.Netcode;
#endif

/// <summary>
/// Spawns two players at spawn points for offline testing.
/// If Netcode (NGO) is running (IsListening), this spawner stays inactive,
/// since player spawning is handled by NetworkManager.
/// </summary>
public class DuelSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawnA;
    public Transform spawnB;
    public bool autoSpawnOnStart = true;

    void Start()
    {
        if (!autoSpawnOnStart) return;

#if UNITY_NETCODE
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            // In netcode mode, spawning is done by NetworkManager per connection.
            return;
        }
#endif
        SpawnOffline();
    }

    public void SpawnOffline()
    {
        if (playerPrefab == null || spawnA == null || spawnB == null)
        {
            Debug.LogWarning("DuelSpawner: Assign playerPrefab and both spawn points.");
            return;
        }

        var p1 = Instantiate(playerPrefab, spawnA.position, spawnA.rotation);
        p1.name = "Player_1_Offline";

        var p2 = Instantiate(playerPrefab, spawnB.position, spawnB.rotation);
        p2.name = "Player_2_Offline";

        // Disable controls on player 2 so it's a dummy target
        var cam = p2.GetComponentInChildren<FirstPersonCamera>(true);
        if (cam != null) cam.gameObject.SetActive(false);
        var move = p2.GetComponent<SimpleMovementController>();
        if (move != null) move.enabled = false;
    }
}
