using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(MeleeWeapon))]
public class PlayerCombatController : MonoBehaviour
{
    public MeleeWeapon weapon;
    public Transform cameraTransform;
    public LayerMask aimMask = ~0;
    public Animator animator;
    public string attackTrigger = "Attack";

    void Awake()
    {
        if (weapon == null) weapon = GetComponent<MeleeWeapon>();
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 dir = GetAimDirection();
            if (animator != null && !string.IsNullOrEmpty(attackTrigger))
                animator.SetTrigger(attackTrigger);
            weapon.StartAttack(gameObject, dir);
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 dir = GetAimDirection();
            if (animator != null && !string.IsNullOrEmpty(attackTrigger))
                animator.SetTrigger(attackTrigger);
            weapon.StartAttack(gameObject, dir);
        }
#endif
    }

    // Called by Animation Event at the frame where the weapon should start sampling hits
    public void OnAnimationAttackStart()
    {
        if (weapon == null) weapon = GetComponent<MeleeWeapon>();
        if (weapon == null) return;
        Vector3 dir = GetAimDirection();
        weapon.StartAttack(gameObject, dir);
    }

    // Called by Animation Event when the attack animation ends (optional)
    public void OnAnimationAttackEnd()
    {
        if (weapon == null) weapon = GetComponent<MeleeWeapon>();
        if (weapon == null) return;
        weapon.EndAttack();
    }

    Vector3 GetAimDirection()
    {
        if (cameraTransform == null) return transform.forward;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out var hit, 100f, aimMask, QueryTriggerInteraction.Ignore))
            return (hit.point - weapon.transform.position).normalized;
        return cameraTransform.forward;
    }
}
