#if UNITY_NETCODE
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class NetworkPlayerController : NetworkBehaviour
{
    public FirstPersonCamera firstPersonCamera;
    public SimpleMovementController movement;
    public PlayerCombatController_Animated combat;

    void Awake()
    {
        if (firstPersonCamera == null) firstPersonCamera = GetComponentInChildren<FirstPersonCamera>(true);
        if (movement == null) movement = GetComponent<SimpleMovementController>();
        if (combat == null) combat = GetComponent<PlayerCombatController_Animated>();
    }

    public override void OnNetworkSpawn()
    {
        // Owner-only input and camera
        if (firstPersonCamera != null)
        {
            firstPersonCamera.enabled = IsOwner;
            
            // Disable entire camera GameObject on non-local players
            firstPersonCamera.gameObject.SetActive(IsOwner);
            
            // Also ensure AudioListener is disabled on non-owners
            var audioListener = firstPersonCamera.GetComponent<AudioListener>();
            if (audioListener == null)
            {
                // Check if it's on the camera's GameObject or any child
                audioListener = firstPersonCamera.GetComponentInChildren<AudioListener>(true);
            }
            if (audioListener != null)
            {
                audioListener.enabled = IsOwner;
            }
        }
        
        if (movement != null) movement.enabled = IsOwner;
        
        // Disable combat input on non-owners
        if (combat != null) combat.enabled = IsOwner;

        // Ensure weapon hit detection is server-authoritative when possible
        if (combat != null && combat.weapon != null)
        {
            combat.weapon.serverAuthority = true;
        }
    }
}
#endif
