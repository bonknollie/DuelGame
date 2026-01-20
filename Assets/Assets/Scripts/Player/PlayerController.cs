using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;        // Ground max speed
    public float accelGround = 45f;      // Very snappy
    public float accelAir = 22f;         // High air control but not Quake
    public float decelGround = 30f;

    [Header("Jump")]
    public float jumpHeight = 2.2f;
    public float gravity = 24f;

    [Header("Coyote")]
    public float coyoteTime = 0.12f;
    public float coyoteBurst = 1.2f;     // forward burst on coyote jump

    [Header("Retro Boost Momentum")]
    public float airPivotKick = 0.75f;   // extra sideways push on direction flip

    [Header("Other Settings")]
    public float groundCheckDist = 0.25f;
    public LayerMask groundMask;
    public bool rotateToMove = true;
    public float rotateSpeed = 10f;
    
    [Header("Animation")]
    public Animator animator;

    [Header("First Person")]
    public bool firstPerson = false;
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float pitchMin = -85f;
    public float pitchMax = 85f;
    public bool lockCursor = true;

    CharacterController controller;
    Vector3 velocity;
    float yVel;
    bool grounded;
    float lastGroundedTime;
    Vector3 lastAirInput;
    float cameraPitch = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Mouse look (first-person)
        if (firstPerson && cameraTransform != null)
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            float mX = GetMouseX() * mouseSensitivity;
            float mY = GetMouseY() * mouseSensitivity;

            // Yaw rotates the player root around Y
            transform.Rotate(Vector3.up, mX);

            // Pitch rotates the camera locally (clamped)
            cameraPitch -= mY;
            cameraPitch = Mathf.Clamp(cameraPitch, pitchMin, pitchMax);
            cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
        }

        UpdateGrounding();
        HandleMovement();
        // Apply gravity first so jump sets a fresh yVel when triggered this frame
        ApplyGravity();
        HandleJump();

        // Ensure velocity uses the resolved vertical speed
        velocity.y = yVel;

        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateGrounding()
    {
        // Use CharacterController's isGrounded as a quick check and a small sphere at the feet
        // for more robust detection across pivots and small geometry.
        Vector3 worldCenter = transform.TransformPoint(controller.center);
        float halfHeight = controller.height * 0.5f;
        // Position the sphere at the bottom of the controller (a little above the actual bottom)
        Vector3 spherePos = worldCenter + Vector3.down * (halfHeight - controller.radius + 0.01f);

        bool sphereHit = Physics.CheckSphere(spherePos, controller.radius + 0.01f, groundMask);
        grounded = controller.isGrounded || sphereHit;

        if (grounded)
        {
            lastGroundedTime = Time.time;
            // Keep a small negative Y so the controller stays snapped to ground and doesn't
            // accumulate large downward velocity.
            if (yVel < 0f)
                yVel = -1f;
        }
    }

    void HandleMovement()
    {
    float h = GetAxisRaw("Horizontal");
    float v = GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0, v).normalized;

        if (firstPerson)
        {
            // In first-person, rotation is handled by mouse look; movement should be relative to camera yaw
            // Do not auto-rotate to movement input.
        }
        else if (rotateToMove && input != Vector3.zero)
        {
            // Rotate only around Y and smooth the rotation for nicer visuals (third-person)
            Quaternion targetRot = Quaternion.LookRotation(new Vector3(input.x, 0f, input.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        Vector3 flatVel = new Vector3(velocity.x, 0, velocity.z);

        Vector3 target;
        if (firstPerson && cameraTransform != null)
        {
            // Move relative to camera yaw
            Vector3 camForward = new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z).normalized;
            Vector3 camRight = new Vector3(cameraTransform.right.x, 0f, cameraTransform.right.z).normalized;
            Vector3 moveDir = (camForward * v + camRight * h);
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();
            target = moveDir * moveSpeed;
        }
        else
        {
            target = input * moveSpeed;
        }

        float accel = grounded ? accelGround : accelAir;
        float decel = grounded ? decelGround : accelAir * 0.5f;

        if (input.magnitude > 0.01f)
        {
            flatVel = Vector3.MoveTowards(flatVel, target, accel * Time.deltaTime);

            // --- Retro Boost Momentum (air pivot kick) ---
            if (!grounded && Vector3.Dot(lastAirInput, input) < -0.2f)
            {
                flatVel += input * airPivotKick; 
            }

            lastAirInput = input;
        }
        else
        {
            flatVel = Vector3.MoveTowards(flatVel, Vector3.zero, decel * Time.deltaTime);
        }

        velocity.x = flatVel.x;
        velocity.z = flatVel.z;

        // Update animator Speed parameter for walk/run
        if (animator != null)
        {
            float speed = new Vector3(velocity.x, 0, velocity.z).magnitude / moveSpeed;
            animator.SetFloat("Speed", speed);
        }
    }

    void HandleJump()
    {
        // Jump with coyote time
    if (GetJumpDown())
        {
            if (grounded || Time.time - lastGroundedTime <= coyoteTime)
            {
                yVel = Mathf.Sqrt(jumpHeight * 2f * gravity);

                // Coyote burst: add a horizontal forward kick when jumping in coyote window
                if (!grounded)
                {
                    Vector3 forwardFlat = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
                    velocity += forwardFlat * coyoteBurst;
                }
            }
        }

    }

    void ApplyGravity()
    {
        // Gravity applies only to vertical velocity. When grounded yVel is clamped in UpdateGrounding.
        yVel -= gravity * Time.deltaTime;
    }

    // Input compatibility helpers: prefer the new Input System when available, otherwise fall back
    float GetAxisRaw(string axisName)
    {
#if ENABLE_INPUT_SYSTEM
        // Horizontal/Vertical using keyboard or gamepad
        if (axisName == "Horizontal")
        {
            float v = 0f;
            if (Gamepad.current != null) v += Gamepad.current.leftStick.x.ReadValue();
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) v -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) v += 1f;
            }
            return Mathf.Clamp(v, -1f, 1f);
        }
        if (axisName == "Vertical")
        {
            float v = 0f;
            if (Gamepad.current != null) v += Gamepad.current.leftStick.y.ReadValue();
            if (Keyboard.current != null)
            {
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v -= 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v += 1f;
            }
            return Mathf.Clamp(v, -1f, 1f);
        }
        // Unknown axis, return 0
        return 0f;
#else
        return Input.GetAxisRaw(axisName);
#endif
}

    float GetMouseX()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.delta.x.ReadValue() * 0.02f; // scale to approximate old axis
        return 0f;
#else
        return Input.GetAxis("Mouse X");
#endif
    }

    float GetMouseY()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.delta.y.ReadValue() * 0.02f;
        return 0f;
#else
        return Input.GetAxis("Mouse Y");
#endif
    }

    bool GetJumpDown()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) return true;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) return true;
        return false;
#else
        return Input.GetButtonDown("Jump");
#endif
    }

}
