using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages 1v1 arena mode: player spawning, match state, victory conditions
/// </summary>
public class ArenaManager : MonoBehaviour
{
    [Header("Arena Setup")]
    public SpawnArea player1SpawnArea;
    public SpawnArea player2SpawnArea;
    public Transform arenaCenter;

    [Header("Player References")]
    private HealthComponent player1Health;
    private HealthComponent player2Health;
    private GameObject player1GameObject;
    private GameObject player2GameObject;

    [Header("Match State")]
    private bool matchActive = false;
    private GameObject matchWinner = null;

    [Header("UI")]
    public ArenaUIManager uiManager;

    public static ArenaManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InitializeArena();
    }

    void InitializeArena()
    {
        // Find or create players
        FindOrCreatePlayers();

        // Find health components
        if (player1GameObject != null)
            player1Health = player1GameObject.GetComponent<HealthComponent>();
        if (player2GameObject != null)
            player2Health = player2GameObject.GetComponent<HealthComponent>();

        // Spawn players at their spawn areas
        SpawnPlayers();

        // Hook up death events
        if (player1Health != null)
            player1Health.onDeath.AddListener(() => OnPlayerDeath(player1GameObject));
        if (player2Health != null)
            player2Health.onDeath.AddListener(() => OnPlayerDeath(player2GameObject));

        // Update UI
        if (uiManager != null)
        {
            uiManager.SetPlayer1Health(player1Health);
            uiManager.SetPlayer2Health(player2Health);
        }

        matchActive = true;
    }

    void FindOrCreatePlayers()
    {
        // Try to find existing players with PlayerController
        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        
        if (allPlayers.Length >= 2)
        {
            player1GameObject = allPlayers[0].gameObject;
            player2GameObject = allPlayers[1].gameObject;
        }
        else if (allPlayers.Length == 1)
        {
            player1GameObject = allPlayers[0].gameObject;
            Debug.LogWarning("ArenaManager: Only found 1 player. Need 2 for 1v1 mode.");
        }
        else
        {
            Debug.LogError("ArenaManager: No players found in scene!");
        }
    }

    void SpawnPlayers()
    {
        if (player1SpawnArea != null && player1GameObject != null)
        {
            Vector3 pos = player1SpawnArea.GetSpawnPosition(0);
            player1GameObject.transform.position = pos;
        }

        if (player2SpawnArea != null && player2GameObject != null)
        {
            Vector3 pos = player2SpawnArea.GetSpawnPosition(0);
            player2GameObject.transform.position = pos;
        }
    }

    void OnPlayerDeath(GameObject deadPlayer)
    {
        if (!matchActive) return;

        matchActive = false;
        matchWinner = deadPlayer == player1GameObject ? player2GameObject : player1GameObject;

        if (uiManager != null)
        {
            string winnerName = matchWinner == player1GameObject ? "Player 1" : "Player 2";
            uiManager.ShowVictoryScreen(winnerName);
        }

        // Disable the other player's input after a delay
        Invoke(nameof(EndMatch), 2f);
    }

    void EndMatch()
    {
        if (uiManager != null)
        {
            uiManager.ShowRestartPrompt();
        }
    }

    public void RestartMatch()
    {
        // Reset players
        if (player1GameObject != null)
        {
            player1GameObject.SetActive(true);
            if (player1Health != null)
                player1Health.ResetHealth();
        }

        if (player2GameObject != null)
        {
            player2GameObject.SetActive(true);
            if (player2Health != null)
                player2Health.ResetHealth();
        }

        matchWinner = null;
        matchActive = false;

        InitializeArena();
    }

    public bool IsMatchActive() => matchActive;
    public GameObject GetWinner() => matchWinner;
}
