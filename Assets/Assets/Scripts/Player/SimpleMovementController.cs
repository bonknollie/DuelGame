using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Simple third-person movement controller for arena combat
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SimpleMovementController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 8f;
    public float groundDrag = 0.1f;

    [Header("Animation")]
    public Animator animator;

    [Header("Rotation")]
    public bool rotateToMove = true;
    public float rotateSpeed = 15f;

    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    private float verticalVelocity = 0f;
    private float gravity = 20f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleRotation();
        UpdateAnimator();
    }

    void HandleInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed) vertical -= 1f;
            if (Keyboard.current.aKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.dKey.isPressed) horizontal += 1f;
        }
#else
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
#endif

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // Movement relative to where player is facing
        Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 right = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
        inputDirection = (forward * vertical + right * horizontal).normalized;

        // Smooth acceleration/deceleration
        if (inputDirection.magnitude > 0.01f)
        {
            moveDirection = Vector3.MoveTowards(moveDirection, inputDirection * moveSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            moveDirection = Vector3.MoveTowards(moveDirection, Vector3.zero, deceleration * Time.deltaTime);
        }
    }

    void HandleMovement()
    {
        // Apply gravity
        if (!controller.isGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        else
        {
            verticalVelocity = -0.1f; // Small negative to keep grounded
        }

        // Move
        Vector3 finalMovement = moveDirection + new Vector3(0f, verticalVelocity, 0f);
        controller.Move(finalMovement * Time.deltaTime);
    }

    void HandleRotation()
    {
        if (rotateToMove && moveDirection.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        // Speed for animation blending (0-2 range)
        float speed = moveDirection.magnitude / moveSpeed;
        animator.SetFloat("Speed", speed);
    }
}
