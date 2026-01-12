using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("The layer where your Auto-Pickup items are (e.g., 'Items')")]
    public LayerMask itemLayer;

    [Tooltip("The layer where NPCs and Chests are (e.g., 'Interactable')")]
    public LayerMask interactableLayer;

    [Header("UI Reference")]
    [Tooltip("The 'Press E' popup icon (InteractionIcon child)")]
    public GameObject interactionIcon;

    private IInteractable interactableInRange = null;

    private void Start()
    {
        // Hide the 'E' prompt at the start
        if (interactionIcon != null) interactionIcon.SetActive(false);
    }

    // --- 1. DETECTION LOGIC ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // A. AUTO-PICKUP LOGIC (Triggered by overlap)
        if (((1 << collision.gameObject.layer) & itemLayer) != 0)
        {
            if (collision.TryGetComponent<Item>(out Item itemToCollect))
            {
                // Safety: Don't pick up if the script is already disabled
                if (!itemToCollect.enabled) return;

                Debug.Log($"Overlapped and collected: {itemToCollect.Name}");

                // Trigger the logic inside Item.cs
                itemToCollect.OnPickupAttempted();

                // Disable the item script immediately to prevent double-pickup
                itemToCollect.enabled = false;
                return;
            }
        }

        // B. INTERACTABLE LOGIC (NPCs, Chests, Doors)
        if (((1 << collision.gameObject.layer) & interactableLayer) != 0)
        {
            if (collision.TryGetComponent(out IInteractable interactable))
            {
                interactableInRange = interactable;

                if (interactableInRange.CanInteract() && interactionIcon != null)
                {
                    interactionIcon.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Clear the reference when the player walks away
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;
            if (interactionIcon != null) interactionIcon.SetActive(false);
        }
    }

    // --- 2. INPUT LOGIC (Button Press) ---
    // Link this to your 'Interact' Action in the Player Input component
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (interactableInRange != null)
        {
            interactableInRange.Interact();

            // Hide icon if the NPC is no longer interactable after talking
            if (interactionIcon != null && !interactableInRange.CanInteract())
            {
                interactionIcon.SetActive(false);
            }
        }
    }
}