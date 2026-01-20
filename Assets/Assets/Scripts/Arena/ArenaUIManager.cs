using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages UI display for arena matches
/// </summary>
public class ArenaUIManager : MonoBehaviour
{
    [Header("Health Displays")]
    public Image player1HealthBar;
    public TextMeshProUGUI player1HealthText;
    public Image player2HealthBar;
    public TextMeshProUGUI player2HealthText;

    [Header("Match Status")]
    public TextMeshProUGUI matchStatusText;
    public Canvas victoryScreenCanvas;
    public TextMeshProUGUI victoryText;
    public Button restartButton;

    private HealthComponent player1Health;
    private HealthComponent player2Health;

    void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (victoryScreenCanvas != null)
            victoryScreenCanvas.gameObject.SetActive(false);
    }

    public void SetPlayer1Health(HealthComponent health)
    {
        player1Health = health;
        if (health != null)
        {
            health.onHealthChanged.AddListener(UpdatePlayer1HealthBar);
            UpdatePlayer1HealthBar(health.GetCurrentHealth());
        }
    }

    public void SetPlayer2Health(HealthComponent health)
    {
        player2Health = health;
        if (health != null)
        {
            health.onHealthChanged.AddListener(UpdatePlayer2HealthBar);
            UpdatePlayer2HealthBar(health.GetCurrentHealth());
        }
    }

    void UpdatePlayer1HealthBar(float currentHealth)
    {
        if (player1Health == null) return;

        float healthPercent = player1Health.GetHealthPercent();
        if (player1HealthBar != null)
            player1HealthBar.fillAmount = healthPercent;

        if (player1HealthText != null)
            player1HealthText.text = $"Player 1: {currentHealth:F0}/{player1Health.maxHealth:F0}";
    }

    void UpdatePlayer2HealthBar(float currentHealth)
    {
        if (player2Health == null) return;

        float healthPercent = player2Health.GetHealthPercent();
        if (player2HealthBar != null)
            player2HealthBar.fillAmount = healthPercent;

        if (player2HealthText != null)
            player2HealthText.text = $"Player 2: {currentHealth:F0}/{player2Health.maxHealth:F0}";
    }

    public void ShowVictoryScreen(string winnerName)
    {
        if (victoryScreenCanvas != null)
        {
            victoryScreenCanvas.gameObject.SetActive(true);
            if (victoryText != null)
                victoryText.text = $"{winnerName} Wins!";
        }
    }

    public void ShowRestartPrompt()
    {
        if (matchStatusText != null)
            matchStatusText.text = "Press R to restart or ESC to quit";
    }

    void OnRestartClicked()
    {
        ArenaManager manager = ArenaManager.Instance;
        if (manager != null)
            manager.RestartMatch();
        
        if (victoryScreenCanvas != null)
            victoryScreenCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        // Quick restart with R key
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnRestartClicked();
        }

        // Quit with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
