using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class HealthComponent : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    [Header("Events")]
    public UnityEvent<float> onHealthChanged;
    public UnityEvent<float> onDamageTaken;
    public UnityEvent onDeath;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Takes damage and applies knockback
    /// </summary>
    public void TakeDamage(float damage, Vector3 knockbackDirection = default, float knockbackForce = 0f)
    {
        if (isDead) return;

        currentHealth -= damage;
        onDamageTaken?.Invoke(damage);
        onHealthChanged?.Invoke(currentHealth);

        // Apply knockback if provided
        if (knockbackForce > 0f)
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null && knockbackDirection != Vector3.zero)
            {
                // Simple knockback by adjusting velocity
                cc.Move(knockbackDirection.normalized * knockbackForce * Time.deltaTime);
            }
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Restore health
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHealthChanged?.Invoke(currentHealth);
    }

    /// <summary>
    /// Reset health to max
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        onHealthChanged?.Invoke(currentHealth);
    }

    /// <summary>
    /// Handle death
    /// </summary>
    void Die()
    {
        if (isDead) return;
        isDead = true;
        onDeath?.Invoke();
        
        // Disable movement and combat
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;
        
        PlayerCombatController pcc = GetComponent<PlayerCombatController>();
        if (pcc != null) pcc.enabled = false;

        // Optionally hide or ragdoll
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Called by MeleeWeapon when damage is applied
    /// </summary>
    public void ApplyDamage(float damage)
    {
        TakeDamage(damage);
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercent() => currentHealth / maxHealth;
    public bool IsDead() => isDead;
}
