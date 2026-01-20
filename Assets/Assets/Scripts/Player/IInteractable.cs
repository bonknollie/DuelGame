using UnityEngine;

/// <summary>
/// Simple interface for interactable world objects.
/// </summary>
public interface IInteractable
{
    /// <summary>Called when the player interacts with this object.</summary>
    void Interact(GameObject interactor = null);
}
