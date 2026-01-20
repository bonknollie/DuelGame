using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Simple first-person camera with mouse look
/// </summary>
public class FirstPersonCamera : MonoBehaviour
{
    [Header("Mouse Look")]
    public Transform playerRoot;  // Drag the IdleArmed object here
    public float mouseSensitivity = 2f;
    [Tooltip("Max yaw turn speed allowed while attacking (deg/sec)")]
    public float maxAttackTurnRate = 120f;
    [Tooltip("Time to smooth yaw (horizontal) rotation. Lower = snappier, Higher = smoother.")]
    public float yawSmoothTime = 0.04f;
    [Tooltip("Time to smooth pitch (vertical) rotation. Lower = snappier, Higher = smoother.")]
    public float pitchSmoothTime = 0.04f;
    public float minPitch = -90f;
    public float maxPitch = 90f;
    public bool lockCursor = true;

    private float pitch = 0f;
    private float targetPitch = 0f;
    private float pitchVel = 0f;
    private float yaw = 0f;
    private float targetYaw = 0f;
    private float yawVel = 0f;
    private PlayerCombatController_Animated combat;

    void Start()
    {
        // Auto-find root if not assigned
        if (playerRoot == null)
        {
            Transform current = transform.parent;
            while (current != null && current.parent != null && current.GetComponent<Animator>() == null)
                current = current.parent;
            playerRoot = current;
        }
        
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Initialize angles from current transforms so there is no snap
        if (playerRoot != null)
            yaw = targetYaw = playerRoot.eulerAngles.y;
        pitch = targetPitch = transform.localEulerAngles.x;

        // Try find combat controller on the root
        if (playerRoot != null)
        {
            combat = playerRoot.GetComponent<PlayerCombatController_Animated>();
            if (combat == null)
                combat = playerRoot.GetComponentInChildren<PlayerCombatController_Animated>();
        }
    }

    void LateUpdate()
    {
        HandleMouseLook();
    }

    void HandleMouseLook()
    {
        Vector2 mouseDelta = GetMouseDelta();
        
        // Accumulate desired yaw/pitch from mouse (with attack turn cap)
        float yawInput = mouseDelta.x * mouseSensitivity;
        if (combat != null && combat.IsAttacking())
        {
            float maxDelta = maxAttackTurnRate * Time.deltaTime;
            yawInput = Mathf.Clamp(yawInput, -maxDelta, maxDelta);
        }
        targetYaw   += yawInput;
        targetPitch -= mouseDelta.y * mouseSensitivity;

        // Clamp vertical only
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        // Smooth the angles for better handling
        yaw   = Mathf.SmoothDampAngle(yaw,   targetYaw,   ref yawVel,   yawSmoothTime);
        pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref pitchVel, pitchSmoothTime);

        // Apply yaw on the root (world up)
        if (playerRoot != null)
            playerRoot.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Rotate camera (pitch)
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // Unlock cursor if ESC pressed
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
#else
        if (Input.GetKeyDown(KeyCode.Escape))
#endif
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }

    Vector2 GetMouseDelta()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            return Mouse.current.delta.ReadValue();
        return Vector2.zero;
#else
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
    }
}
