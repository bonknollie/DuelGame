#if UNITY_NETCODE
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class NetworkCombatAdapter : NetworkBehaviour
{
    // Simplified: just sync attacks via NetworkAnimator
    // Owner-only input prevents weirdness
}
#endif
