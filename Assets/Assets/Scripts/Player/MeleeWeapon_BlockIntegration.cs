using UnityEngine;

/// <summary>
/// INTEGRATION GUIDE for MeleeWeapon with block detection
/// Add this logic to the existing MeleeWeapon.cs Sweep() method
/// </summary>
public class MeleeWeapon_BlockDetection_Example : MonoBehaviour
{
    // This shows how to modify the existing Sweep() method in MeleeWeapon.cs
    
    // EXISTING CODE in MeleeWeapon.cs Sweep() method:
    /*
    void Sweep(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist <= 0f) return;

        dir /= dist;
        RaycastHit[] hits = Physics.SphereCastAll(from, radius, dir, dist, hitMask, QueryTriggerInteraction.Collide);
        foreach (var h in hits)
        {
            if (h.collider == null) continue;
            var go = h.collider.gameObject;
            if (go == owner) continue;
            int id = go.GetInstanceID();
            if (hitSet.Contains(id)) continue;
            hitSet.Add(id);

            // REPLACE THIS SECTION with code below:
            // go.SendMessage("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);
            
            // NEW CODE - Add block detection:
            DefenseController defense = go.GetComponent<DefenseController>();
            if (defense != null && defense.IsBlockingWeaponHit(h.point))
            {
                // BLOCKED! Interrupt attacker's animation and apply stun
                
                // Tell defender they blocked
                defense.OnWeaponBlocked();
                
                // Tell attacker they were blocked
                PlayerCombatController_Animated attackerCombat = owner.GetComponent<PlayerCombatController_Animated>();
                if (attackerCombat != null)
                {
                    attackerCombat.OnAttackBlocked(); // This plays stun animation
                }
                
                // Apply small knockback to attacker (optional)
                CharacterController cc = owner.GetComponent<CharacterController>();
                if (cc != null)
                {
                    Vector3 knockback = -dir * 5f; // Pushes back
                    cc.Move(knockback * Time.deltaTime);
                }
                
                // Visual/audio feedback
                CreateBlockEffect(h.point, h.normal);
                
                // Skip damage application - blocked!
                continue;
            }
            
            // Not blocked - apply normal damage
            go.SendMessage("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);

            // ... rest of existing code ...
            hitPoints.Add(h.point);

            var rb = h.rigidbody ?? h.collider.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
                rb.AddForceAtPosition(dir * 40f, h.point, ForceMode.Impulse);

            Debug.DrawLine(from, h.point, Color.red, 1f);
        }
    }
    */
    
    void CreateBlockEffect(Vector3 hitPoint, Vector3 normal)
    {
        // Create visual feedback for block hit
        Debug.Log("BLOCK HIT at " + hitPoint);
        
        // TODO: Add here:
        // - Particle effect (sparks, shield impact)
        // - Sound effect (clang, metal hit)
        // - Screen shake
        // - Decal/mark
    }
}
