using System.Collections.Generic;
using UnityEngine;
#if UNITY_NETCODE
using Unity.Netcode;
#endif

public class MeleeWeapon : MonoBehaviour
{
    [Header("References")]
    public Transform pivot; // pivot point for swing (weapon root)
    public Transform tip;   // tip transform (local offset used if provided)

    [Header("Swing")]
    public float sweepAngle = 120f; // total sweep in degrees
    public float duration = 0.45f;  // seconds
    public AnimationCurve swingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool orientToDirection = true; // rotate swing to face given attack direction
    public bool drivenByAnimation = false; // when true, animation should move pivot/tip; MeleeWeapon only samples
    public bool clampYaw = true; // limit how far the weapon will yaw to face aim direction
    public float maxYawAngle = 90f; // degrees
    public bool smoothYaw = true; // smoothly rotate towards target yaw to avoid snapping
    public float yawSmoothSpeed = 720f; // degrees per second for smoothing

    [Header("Hit")]
    public float radius = 0.12f;    // sphere radius used for sweep
    public LayerMask hitMask = ~0;  // layers to hit
    public float damage = 25f;
    [Tooltip("When true and Netcode is present, only the server runs hit detection.")]
    public bool serverAuthority = false;

    GameObject owner;
    bool attacking;
    float startTime;
    Vector3 prevTipPos;
    float baseYawOffset = 0f; // legacy storage (kept for compatibility)
    float targetYawOffset = 0f; // desired yaw offset computed on attack start
    float currentYawOffset = 0f; // used during sampling (smoothed towards target)
    HashSet<int> hitSet = new HashSet<int>();
    // Debug / visualization
    [Header("Debug")]
    public bool debugAlwaysShowGizmos = false;
    public bool debugShowSamples = true;
    public Color gizmoColor = Color.yellow;

    [Header("Segmentation")]
    public bool useSegments = true; // sample multiple points along the blade
    [Range(1, 16)]
    public int segmentCount = 4;

    // runtime debug buffers
    List<Vector3> sampledPositions = new List<Vector3>();
    List<Vector3> hitPoints = new List<Vector3>();
    List<Vector3>[] sampledSegmentPaths;
    Vector3[] prevSegmentPos;

    /// <summary>Start an attack. 'direction' is used only to orient the pivot if needed.</summary>
    public void StartAttack(GameObject owner, Vector3 direction)
    {
#if UNITY_NETCODE
        if (serverAuthority && NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
            return;
#endif
        if (attacking) return;
        if (pivot == null || tip == null)
        {
            Debug.LogWarning("MeleeWeapon: pivot or tip not assigned — attack aborted.");
            return;
        }
        this.owner = owner;
        attacking = true;
        startTime = Time.time;
        hitSet.Clear();
        sampledPositions.Clear();
        hitPoints.Clear();
        // compute yaw offset so sweep faces the requested direction (skip if animation drives orientation)
        targetYawOffset = 0f;
        if (!drivenByAnimation && orientToDirection && direction.sqrMagnitude > 0.001f)
        {
            Vector3 f = Vector3.ProjectOnPlane(pivot.forward, pivot.up).normalized;
            Vector3 d = Vector3.ProjectOnPlane(direction, pivot.up).normalized;
            if (f.sqrMagnitude > 0.001f && d.sqrMagnitude > 0.001f)
                targetYawOffset = Vector3.SignedAngle(f, d, pivot.up);
        }

        if (clampYaw)
            targetYawOffset = Mathf.Clamp(targetYawOffset, -Mathf.Abs(maxYawAngle), Mathf.Abs(maxYawAngle));

        baseYawOffset = targetYawOffset;
        if (!smoothYaw)
            currentYawOffset = targetYawOffset;

        prevTipPos = SampleTipWorldPosition(0f);
        sampledPositions.Add(prevTipPos);
        // init segments
        if (useSegments)
        {
            segmentCount = Mathf.Max(1, segmentCount);
            prevSegmentPos = new Vector3[segmentCount];
            sampledSegmentPaths = new List<Vector3>[segmentCount];
            for (int i = 0; i < segmentCount; i++)
            {
                prevSegmentPos[i] = SampleSegmentWorldPosition(0f, i);
                sampledSegmentPaths[i] = new List<Vector3> { prevSegmentPos[i] };
            }
        }
    }

    void FixedUpdate()
    {
#if UNITY_NETCODE
        if (serverAuthority && NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
            return;
#endif
        if (!attacking) return;

        float t = (Time.time - startTime) / Mathf.Max(0.0001f, duration);
        if (t >= 1f)
        {
            // final sample + sweep, then finish
            Vector3 cur = SampleTipWorldPosition(1f);
            Sweep(prevTipPos, cur);
            attacking = false;
            return;
        }

        Vector3 curTip = SampleTipWorldPosition(t);
        Sweep(prevTipPos, curTip);
        prevTipPos = curTip;
        if (debugShowSamples) sampledPositions.Add(curTip);

        if (useSegments && prevSegmentPos != null)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                Vector3 cur = SampleSegmentWorldPosition(t, i);
                Sweep(prevSegmentPos[i], cur);
                prevSegmentPos[i] = cur;
                if (sampledSegmentPaths != null && sampledSegmentPaths.Length > i)
                    sampledSegmentPaths[i].Add(cur);
            }
        }

        // smooth yaw towards target if enabled
        if (smoothYaw)
        {
            float maxDelta = yawSmoothSpeed * Time.fixedDeltaTime;
            currentYawOffset = Mathf.MoveTowards(currentYawOffset, targetYawOffset, maxDelta);
        }
    }

    Vector3 SampleTipWorldPosition(float normalizedT)
    {
        if (pivot == null || tip == null)
            return transform.position;

        float u = swingCurve.Evaluate(Mathf.Clamp01(normalizedT));
        float angle = Mathf.Lerp(-sweepAngle * 0.5f, sweepAngle * 0.5f, u);
        // use currentYawOffset (smoothed) if available, otherwise fall back to baseYawOffset
        float yaw = currentYawOffset != 0f || smoothYaw ? currentYawOffset : baseYawOffset;
        angle += yaw;

        // rotate the tip's world offset around the pivot's up axis
        Vector3 worldTip = tip.position;
        Vector3 offset = worldTip - pivot.position;
        Vector3 world = pivot.position + Quaternion.AngleAxis(angle, pivot.up) * offset;
        return world;
    }

    Vector3 SampleSegmentWorldPosition(float normalizedT, int segmentIndex)
    {
        if (pivot == null || tip == null) return transform.position;
        int segCount = Mathf.Max(1, segmentCount);
        float u = swingCurve.Evaluate(Mathf.Clamp01(normalizedT));
        float angle = Mathf.Lerp(-sweepAngle * 0.5f, sweepAngle * 0.5f, u);
        angle += baseYawOffset;

        Vector3 offset = tip.position - pivot.position;
        float frac = (segmentIndex + 1) / (float)segCount; // 1..1.0
        Vector3 world = pivot.position + Quaternion.AngleAxis(angle, pivot.up) * (offset * frac);
        return world;
    }

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

            // Check if target is blocking
            DefenseController defenseController = go.GetComponent<DefenseController>();
            if (defenseController != null && defenseController.IsBlockingWeaponHit(h.point))
            {
                // Attack is blocked
                defenseController.OnWeaponBlocked();
                
                // Notify attacker that their attack was blocked
                if (owner != null)
                {
                    PlayerCombatController_Animated combatController = owner.GetComponent<PlayerCombatController_Animated>();
                    if (combatController != null)
                        combatController.OnAttackBlocked();
                }
                
                Debug.DrawLine(from, h.point, Color.blue, 1f); // Blue = blocked
                continue;
            }

            // Attack connects - apply damage
            go.SendMessage("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);

            // record hit for debug
            hitPoints.Add(h.point);

            // Simple physics impulse for visible feedback
            var rb = h.rigidbody ?? h.collider.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
                rb.AddForceAtPosition(dir * 40f, h.point, ForceMode.Impulse);

            Debug.DrawLine(from, h.point, Color.red, 1f);
        }
    }

    /// <summary>Forcefully end the current attack (useful when animation controls timing).</summary>
    public void EndAttack()
    {
        attacking = false;
        hitSet.Clear();
        sampledPositions.Clear();
        hitPoints.Clear();
    }

    void OnDrawGizmosSelected()
    {
        DrawDebugGizmos();
    }

    void OnDrawGizmos()
    {
        if (debugAlwaysShowGizmos) DrawDebugGizmos();
    }

    void DrawDebugGizmos()
    {
        if (pivot == null || tip == null) return;
        Gizmos.color = gizmoColor;

        // draw sweep endpoints
        Vector3 worldTip = tip.position;
        Vector3 offset = worldTip - pivot.position;
        float yaw = currentYawOffset != 0f || smoothYaw ? currentYawOffset : baseYawOffset;
        Vector3 a = pivot.position + Quaternion.AngleAxis(-sweepAngle * 0.5f + yaw, pivot.up) * offset;
        Vector3 b = pivot.position + Quaternion.AngleAxis(sweepAngle * 0.5f + yaw, pivot.up) * offset;
        Gizmos.DrawSphere(a, radius);
        Gizmos.DrawSphere(b, radius);
        Gizmos.DrawLine(pivot.position, a);
        Gizmos.DrawLine(pivot.position, b);

        // draw sampled tip path
        if (sampledPositions != null && sampledPositions.Count > 0 && debugShowSamples)
        {
            Gizmos.color = Color.cyan;
            Vector3 prev = sampledPositions[0];
            for (int i = 1; i < sampledPositions.Count; i++)
            {
                Vector3 cur = sampledPositions[i];
                Gizmos.DrawLine(prev, cur);
                Gizmos.DrawSphere(cur, radius * 0.5f);
                prev = cur;
            }
        }

        // draw segmented paths
        if (useSegments && sampledSegmentPaths != null)
        {
            for (int s = 0; s < sampledSegmentPaths.Length; s++)
            {
                var path = sampledSegmentPaths[s];
                if (path == null || path.Count == 0) continue;
                Gizmos.color = Color.magenta;
                Vector3 pprev = path[0];
                for (int i = 1; i < path.Count; i++)
                {
                    Vector3 pcur = path[i];
                    Gizmos.DrawLine(pprev, pcur);
                    Gizmos.DrawSphere(pcur, radius * 0.35f);
                    pprev = pcur;
                }
            }
        }

        // draw hit points
        if (hitPoints != null && hitPoints.Count > 0)
        {
            Gizmos.color = Color.red;
            foreach (var p in hitPoints)
                Gizmos.DrawSphere(p, radius * 0.8f);
        }
    }
}
