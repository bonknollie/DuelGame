using UnityEngine;

/// <summary>
/// Debug utility for 1v1 Arena mode - attach to a GameObject and use in OnGUI to display debug info
/// </summary>
public class ArenaDebugger : MonoBehaviour
{
    public bool showDebug = true;
    private HealthComponent player1Health;
    private HealthComponent player2Health;
    private ArenaManager arenaManager;

    void Start()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        if (players.Length > 0)
            player1Health = players[0].GetComponent<HealthComponent>();
        if (players.Length > 1)
            player2Health = players[1].GetComponent<HealthComponent>();

        arenaManager = ArenaManager.Instance;
    }

    void OnGUI()
    {
        if (!showDebug) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.Label("=== ARENA DEBUG ===", new GUIStyle(GUI.skin.label) { fontSize = 16 });

        if (player1Health != null)
        {
            GUILayout.Label($"Player 1 Health: {player1Health.GetCurrentHealth():F1}/{player1Health.maxHealth}");
            GUILayout.Label($"Player 1 Dead: {player1Health.IsDead()}");
        }

        if (player2Health != null)
        {
            GUILayout.Label($"Player 2 Health: {player2Health.GetCurrentHealth():F1}/{player2Health.maxHealth}");
            GUILayout.Label($"Player 2 Dead: {player2Health.IsDead()}");
        }

        if (arenaManager != null)
        {
            GUILayout.Label($"Match Active: {arenaManager.IsMatchActive()}");
            if (arenaManager.GetWinner() != null)
                GUILayout.Label($"Winner: {arenaManager.GetWinner().name}");
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Test Damage Player 1 (25)"))
        {
            if (player1Health != null)
                player1Health.TakeDamage(25);
        }

        if (GUILayout.Button("Test Damage Player 2 (25)"))
        {
            if (player2Health != null)
                player2Health.TakeDamage(25);
        }

        if (GUILayout.Button("Reset Match"))
        {
            if (arenaManager != null)
                arenaManager.RestartMatch();
        }

        if (GUILayout.Button("Kill Player 1"))
        {
            if (player1Health != null)
                player1Health.TakeDamage(player1Health.maxHealth * 2);
        }

        if (GUILayout.Button("Kill Player 2"))
        {
            if (player2Health != null)
                player2Health.TakeDamage(player2Health.maxHealth * 2);
        }

        showDebug = GUILayout.Toggle(showDebug, "Show Debug");

        GUILayout.EndArea();
    }
}
