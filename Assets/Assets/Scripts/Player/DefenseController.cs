using UnityEngine;

/// <summary>
/// Defense/Blocking system that reduces damage when active
/// Integrates with HealthComponent for damage reduction
/// </summary>
public class DefenseController : MonoBehaviour
{
    [Header("Defense Settings")]
    public float damageReduction = 0.5f; // 50% damage reduction when defending
    public float blockStunDuration = 0.2f; // How long to shake on block
    
    private bool isDefending = false;
    private HealthComponent healthComponent;
    private PlayerCombatController_Animated combatController;

    void Awake()
    {
        healthComponent = GetComponent<HealthComponent>();
        combatController = GetComponent<PlayerCombatController_Animated>();
    }

    void Update()
    {
        // Check if defending from combat controller
        if (combatController != null)
        {
            isDefending = combatController.IsDefending();
        }
    }

    /// <summary>
    /// Check if currently defending (for blocking weapon hits)
    /// </summary>
    public bool IsBlockingWeaponHit(Vector3 hitPoint)
    {
        return isDefending;
    }

    /// <summary>
    /// Called when weapon successfully blocks (hits during defense)
    /// </summary>
    public void OnWeaponBlocked()
    {
        OnBlockHit();
    }

    /// <summary>
    /// Apply damage with defense consideration
    /// Called by MeleeWeapon instead of direct ApplyDamage
    /// </summary>
    public void TakeDamageWithDefense(float damage)
    {
        if (isDefending)
        {
            // Reduce damage while defending
            float reducedDamage = damage * (1f - damageReduction);
            
            // Optional: add visual/audio feedback for successful block
            OnBlockHit();
            
            // Apply reduced damage
            if (healthComponent != null)
                healthComponent.TakeDamage(reducedDamage);
        }
        else
        {
            // Take full damage if not defending
            if (healthComponent != null)
                healthComponent.TakeDamage(damage);
        }
    }

    void OnBlockHit()
    {
        // Visual feedback - camera shake, particle effects, etc.
        Debug.Log($"{gameObject.name} blocked an attack! Damage reduced by {damageReduction * 100}%");
        
        // Optional: Add camera shake
        if (Camera.main != null)
        {
            StartCoroutine(CameraShake(blockStunDuration));
        }
    }

    System.Collections.IEnumerator CameraShake(float duration)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-0.1f, 0.1f);
            float y = Random.Range(-0.1f, 0.1f);
            Camera.main.transform.localPosition = originalPos + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Camera.main.transform.localPosition = originalPos;
    }

    public bool IsDefending() => isDefending;
    public float GetDamageReduction() => damageReduction;
}
