using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerItemCollector : MonoBehaviour
{
    [Header("Settings")]
    public bool autoPickup = true;

    [Tooltip("Select the 'Items' layer here in the inspector.")]
    public LayerMask itemLayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Guard Clauses: Early exits for efficiency
        if (!autoPickup) return;

        // 2. Layer Mask Filter: Ensure the object is on the 'Items' layer
        // This is a bitwise check: (1 << layer) shifts the bit to the layer index
        if (((1 << collision.gameObject.layer) & itemLayer) != 0)
        {
            ProcessPickup(collision);
        }
    }

    private void ProcessPickup(Collider2D collision)
    {
        // 3. Component Check: Using TryGetComponent is faster than GetComponent
        if (collision.TryGetComponent<Item>(out Item itemToCollect))
        {
            // 4. State Safety: Ensure we don't pick up an item that is already 'dead'
            // or being destroyed in this frame.
            if (!itemToCollect.enabled || itemToCollect.gameObject == null) return;

            // 5. Communication: Tell the Item it's time to be collected.
            // We disable the component immediately to prevent 'Double Pickup' bugs.
            itemToCollect.enabled = false;

            itemToCollect.OnPickupAttempted();

            Debug.Log($"<color=cyan>PlayerItemCollector:</color> Triggered pickup for {itemToCollect.Name}");
        }
    }
}