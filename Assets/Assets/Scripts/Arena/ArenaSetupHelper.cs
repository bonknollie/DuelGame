using UnityEngine;

/// <summary>
/// Simple helper to set up a basic arena with two spawn areas
/// Attach to an empty GameObject in your scene
/// </summary>
public class ArenaSetupHelper : MonoBehaviour
{
    [Header("Quick Setup")]
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    
    [Header("Spawn Positions")]
    public Vector3 player1SpawnPos = new Vector3(-10f, 1f, 0f);
    public Vector3 player2SpawnPos = new Vector3(10f, 1f, 0f);

    [Header("Arena Bounds")]
    public Vector3 arenaSize = new Vector3(30f, 20f, 30f);
    public Vector3 arenaCenter = Vector3.zero;

    public void SetupArena()
    {
        Debug.Log("Setting up arena...");

        // Create Player1 if not exists
        GameObject p1 = FindObjectOfType<PlayerController>()?.gameObject;
        if (p1 == null && player1Prefab != null)
        {
            p1 = Instantiate(player1Prefab, player1SpawnPos, Quaternion.identity);
            p1.name = "Player1";
        }

        // Create Player2 if not exists
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        GameObject p2 = players.Length > 1 ? players[1].gameObject : null;
        if (p2 == null && player2Prefab != null)
        {
            p2 = Instantiate(player2Prefab, player2SpawnPos, Quaternion.identity);
            p2.name = "Player2";
        }

        // Ensure both have HealthComponent
        if (p1 != null && p1.GetComponent<HealthComponent>() == null)
            p1.AddComponent<HealthComponent>();
        if (p2 != null && p2.GetComponent<HealthComponent>() == null)
            p2.AddComponent<HealthComponent>();

        // Find or create ArenaManager
        ArenaManager manager = FindObjectOfType<ArenaManager>();
        if (manager == null)
        {
            GameObject arenaObj = new GameObject("ArenaManager");
            manager = arenaObj.AddComponent<ArenaManager>();
        }

        // Find or create UI
        ArenaUIManager uiManager = FindObjectOfType<ArenaUIManager>();
        if (uiManager == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            
            GameObject uiObj = new GameObject("ArenaUI");
            uiObj.transform.SetParent(canvas.transform, false);
            uiManager = uiObj.AddComponent<ArenaUIManager>();
        }

        Debug.Log("Arena setup complete!");
    }
}
