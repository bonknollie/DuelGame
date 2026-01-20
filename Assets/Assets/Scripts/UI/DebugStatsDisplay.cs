using UnityEngine;

/// <summary>
/// Simple debug overlay displaying player health and stamina in top-left corner
/// </summary>
public class DebugStatsDisplay : MonoBehaviour
{
    private HealthComponent healthComponent;
    private PlayerCombatController_Animated combatController;
    private GUIStyle labelStyle;

    void Start()
    {
        // Find local player
        healthComponent = FindObjectOfType<HealthComponent>();
        combatController = FindObjectOfType<PlayerCombatController_Animated>();
        
        // Setup GUI style
        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
    }

    void OnGUI()
    {
        if (healthComponent == null || combatController == null) return;

        // Initialize style if needed
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }

        // Top-left corner, start at (10, 10)
        float health = healthComponent.GetCurrentHealth();
        float maxHealth = healthComponent.GetMaxHealth();
        float stamina = combatController.currentStamina;
        float maxStamina = combatController.maxStamina;

        string displayText = $"Health: {health:F0} / {maxHealth:F0}\nStamina: {stamina:F0} / {maxStamina:F0}";
        
        GUI.Label(new Rect(10, 10, 300, 100), displayText, labelStyle);
    }
}
