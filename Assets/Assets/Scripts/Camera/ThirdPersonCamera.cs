using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Simple third-person camera controller with mouse look
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Camera")]
    public Transform player;
    public float distance = 5f;
    public float height = 2f;
    public float smoothSpeed = 5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;
    public float maxHorizontalAngle = 90f;  // Clamp camera behind player (0-90 degrees left/right)
    
    private float horizontalAngle = 0f;
    private float verticalAngle = 20f;

    void LateUpdate()
    {
        if (player == null) return;

        HandleMouseLook();
        UpdateCameraPosition();
    }

    void HandleMouseLook()
    {
        float mouseX = GetMouseDelta().x;
        float mouseY = GetMouseDelta().y;

        horizontalAngle += mouseX * mouseSensitivity;
        verticalAngle -= mouseY * mouseSensitivity;
        
        // Clamp angles
        horizontalAngle = Mathf.Clamp(horizontalAngle, -maxHorizontalAngle, maxHorizontalAngle);
        verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);
    }

    void UpdateCameraPosition()
    {
        // Calculate desired position based on angles
        Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0f);
        Vector3 offset = rotation * new Vector3(0f, height, -distance);  // Negative distance = behind player
        Vector3 targetPos = player.position + offset;

        // Smooth interpolation
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

        // Look at player
        transform.LookAt(player.position + Vector3.up * (height * 0.5f));
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
