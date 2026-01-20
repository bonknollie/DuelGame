using UnityEngine;

[AddComponentMenu("Spawn/SpawnManager")]
public class SpawnManager : MonoBehaviour
{
    public SpawnArea spawnArea;
    public int spawnIndex = 0;
    public bool moveLocalPlayerOnStart = true;
    public string playerTag = "Player"; // optional: tag to find the local player

    void Start()
    {
        if (spawnArea == null)
        {
            Debug.LogWarning("SpawnManager: No SpawnArea assigned.");
            return;
        }

        if (!moveLocalPlayerOnStart) return;

        // Find local player GameObject
        GameObject player = null;
        if (!string.IsNullOrEmpty(playerTag))
        {
            var go = GameObject.FindWithTag(playerTag);
            if (go != null) player = go;
        }

        if (player == null)
        {
            // fallback: first object with PlayerController
            var pc = FindObjectOfType<PlayerController>();
            if (pc != null) player = pc.gameObject;
        }

        if (player == null)
        {
            Debug.LogWarning("SpawnManager: No player found to move.");
            return;
        }

        var pos = spawnArea.GetSpawnPosition(spawnIndex);
        player.transform.position = pos;
        // Optionally zero out velocity when using CharacterController
        var cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            // CharacterController has internal velocity; moving transform is enough for spawn.
        }

        Debug.Log("SpawnManager: moved player to spawn at " + pos);
    }
}
