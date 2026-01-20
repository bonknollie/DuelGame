using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_NETCODE
using Unity.Netcode;
#endif

/// <summary>
/// Enhanced PlayerCombatController with animation support for attacks and defense
/// </summary>
public class PlayerCombatController_Animated : MonoBehaviour
{
    [Header("Combat")]
    public MeleeWeapon weapon;
    public Transform cameraTransform;
    public LayerMask aimMask = ~0;

    [Header("Animator")]
    public Animator animator;
    public string attackTrigger = "Attack";
    public string defenseTrigger = "Defense";
    public string attackBlockedTrigger = "AttackBlocked";
    public string idleState = "Idle";

    [Header("Attack Settings")]
    public float attackCooldown = 0.5f;
    private float lastAttackTime = -1f;
    private bool isAttacking = false;

    [Header("Defense Settings")]
    public float defenseDuration = 1f;
    private bool isDefending = false;
    private float defenseEndTime = 0f;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float attackCost = 25f;
    public float defenseCostPerSecond = 10f;
    public float staminaRegenPerSecond = 15f;

    private bool isNetworked = false;

    void Awake()
    {
        if (weapon == null) weapon = GetComponent<MeleeWeapon>();
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        if (animator == null) animator = GetComponent<Animator>();
        
#if UNITY_NETCODE
        var netObj = GetComponent<NetworkObject>();
        isNetworked = netObj != null;
#endif
    }

    void Update()
    {
        UpdateStamina();
        HandleAttack();
        HandleDefense();
        UpdateAnimatorParameters();
    }

    void UpdateStamina()
    {
        if (isDefending)
        {
            // Drain stamina while defending
            currentStamina -= defenseCostPerSecond * Time.deltaTime;
            if (currentStamina < 0) currentStamina = 0;

            // Force end defense if out of stamina
            if (currentStamina <= 0)
            {
                EndDefense();
            }
        }
        else
        {
            // Regenerate stamina when not defending
            currentStamina += staminaRegenPerSecond * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }
    }

    void HandleAttack()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        bool attackPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        bool attackPressed = Input.GetMouseButtonDown(0);
#endif

#if UNITY_NETCODE
        // If networked, only allow owner to trigger attacks
        if (isNetworked)
        {
            var netObj = GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsOwner)
                return;
        }
#endif

        if (attackPressed && Time.time >= lastAttackTime + attackCooldown && !isDefending && currentStamina >= attackCost)
        {
            PerformAttack();
        }
    }

    void PerformAttack()
    {
        currentStamina -= attackCost;
        lastAttackTime = Time.time;
        Vector3 dir = GetAimDirection();
        
        isAttacking = true;
        
        // Trigger animation (syncs via NetworkAnimator on all clients)
        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
            animator.SetTrigger(attackTrigger);
        
        // Start weapon attack locally
        weapon.StartAttack(gameObject, dir);
    }

    // Server-side: actually trigger the animation
    public void ServerTriggerAttackAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
            animator.SetTrigger(attackTrigger);
    }

    // Server-authoritative start for attacks
    public bool ServerTryStartAttack(Vector3 forwardOverride)
    {
        // Validate server-side cooldown and stamina
        if (Time.time < lastAttackTime + attackCooldown) return false;
        if (isDefending) return false;
        if (currentStamina < attackCost) return false;

        currentStamina -= attackCost;
        lastAttackTime = Time.time;
        isAttacking = true;

        Vector3 dir = forwardOverride.sqrMagnitude > 0.0001f ? forwardOverride.normalized : transform.forward;
        
        // Trigger animation on server (syncs via NetworkAnimator)
        ServerTriggerAttackAnimation();
        
        weapon.StartAttack(gameObject, dir);
        return true;
    }

    void HandleDefense()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        bool defendPressed = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        bool defendReleased = Mouse.current == null || !Mouse.current.rightButton.isPressed;
#else
        bool defendPressed = Input.GetMouseButtonDown(1);  // 1 = right mouse button
        bool defendReleased = !Input.GetMouseButton(1);
#endif

        // Start defense (only if has stamina)
        if (defendPressed && !isDefending && currentStamina > 0)
        {
            StartDefense();
        }

        // End defense
        if (defendReleased && isDefending)
        {
            EndDefense();
        }

        // Auto-end defense after duration
        if (isDefending && Time.time >= defenseEndTime)
        {
            EndDefense();
        }
    }

    void StartDefense()
    {
        isDefending = true;
        defenseEndTime = Time.time + defenseDuration;
        
        if (animator != null && !string.IsNullOrEmpty(defenseTrigger))
            animator.SetTrigger(defenseTrigger);
    }

    void EndDefense()
    {
        isDefending = false;
        
        if (animator != null)
            animator.ResetTrigger(defenseTrigger);
    }

    void UpdateAnimatorParameters()
    {
        if (animator == null) return;
        
        // Reserved for future animator parameters (like Speed from movement controller)
    }

    // Called by Animation Event at the frame where weapon should hit
    public void OnAnimationAttackStart()
    {
        if (weapon == null) weapon = GetComponent<MeleeWeapon>();
        if (weapon == null) return;
        
        Vector3 dir = GetAimDirection();
        weapon.StartAttack(gameObject, dir);
    }

    // Called by Animation Event when attack animation ends
    public void OnAnimationAttackEnd()
    {
        if (weapon == null) weapon = GetComponent<MeleeWeapon>();
        if (weapon == null) return;
        
        weapon.EndAttack();
        isAttacking = false;
    }

    /// <summary>
    /// Called by MeleeWeapon when attack is blocked by defender
    /// Interrupts current attack and plays stun animation
    /// </summary>
    public void OnAttackBlocked()
    {
        // Stop current attack immediately
        if (weapon != null)
            weapon.EndAttack();
        
        // Play block/stun animation
        if (animator != null && !string.IsNullOrEmpty(attackBlockedTrigger))
            animator.SetTrigger(attackBlockedTrigger);
        
        // Reset attack cooldown slightly or apply additional cooldown
        lastAttackTime = Time.time - (attackCooldown * 0.5f); // Can attack sooner after stun
        isAttacking = false;
    }

    // Called by Animation Event when defense ends
    public void OnAnimationDefenseEnd()
    {
        EndDefense();
    }

    Vector3 GetAimDirection()
    {
        if (cameraTransform == null) return transform.forward;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out var hit, 100f, aimMask, QueryTriggerInteraction.Ignore))
            return (hit.point - weapon.transform.position).normalized;
        return cameraTransform.forward;
    }

    public bool IsDefending() => isDefending;
    public bool CanAttack() => Time.time >= lastAttackTime + attackCooldown;
    public float GetStaminaPercent() => currentStamina / maxStamina;
    public bool IsAttacking() => isAttacking;
    public System.Action AttackPerformed;
}
