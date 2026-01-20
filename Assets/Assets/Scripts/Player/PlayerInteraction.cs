using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Player-side interaction component. Raycasts from camera and interacts with IInteractable targets.
/// Attach to your player and assign a camera Transform.
/// Optionally assign an on-screen prompt GameObject to show when looking at an interactable.
/// </summary>
[AddComponentMenu("Interaction/PlayerInteraction")]
public class PlayerInteraction : MonoBehaviour
{
    public Transform cameraTransform;
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.E;
    public GameObject promptObject; // optional: UI element to enable while looking at interactable

    void Start()
    {
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (cameraTransform == null) return;

        // Raycast to check for interactable
        Ray r = new Ray(cameraTransform.position, cameraTransform.forward);
        bool found = false;
        if (Physics.Raycast(r, out RaycastHit hit, interactRange))
        {
            var interact = hit.collider.GetComponentInParent<IInteractable>();
            if (interact != null) found = true;
        }

        if (promptObject != null) promptObject.SetActive(found);

        if (!found) return;

        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) pressed = true;
#else
        if (Input.GetKeyDown(interactKey)) pressed = true;
#endif

        if (!pressed) return;

        // Perform interact
        if (Physics.Raycast(r, out hit, interactRange))
        {
            var interact = hit.collider.GetComponentInParent<IInteractable>();
            if (interact != null)
            {
                interact.Interact(gameObject);
            }
        }
    }
}
