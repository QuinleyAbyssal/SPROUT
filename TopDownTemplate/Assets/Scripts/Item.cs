using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    // --- STATIC ITEM DEFINITION (Set in Prefab Inspector) ---
    // These fields are public for the Inspector but treated as constant data.
    public int ID; // Keep this as '5' for all Tincans (Quest System uses this)
    public string worldID; // Set this to "Can_Area1_1", "Can_Area1_2", etc.

    public string Name;

    public int MaxStackSize = 99;

    // --- DYNAMIC ITEM STATE (Changes at Runtime) ---
    // This is the only field that changes once the game starts.
    public int quantity = 1;

    private TMP_Text quantityText;

    private void Awake()
    {
        quantityText = GetComponentInChildren<TMP_Text>();
        UpdateQuantityDisplay();
    }

    private void Start()
    {
        // FIX: If the item is in the UI layer, do NOT hide it.
        if (gameObject.layer == LayerMask.NameToLayer("UI")) return;

        // Check the unique worldID for world items
        if (WorldStateManager.Instance != null && WorldStateManager.Instance.IsItemCollected(this.worldID))
        {
            gameObject.SetActive(false);
        }
    }

    public void OnPickupAttempted()
    {
        InventoryController.Instance?.HandleItemPickup(this);

        if (!string.IsNullOrEmpty(worldID))
        {
            WorldStateManager.Instance?.MarkCollected(worldID);
            // Turn it off immediately so it doesn't stay on the ground
            gameObject.SetActive(false);
        }
    }

    public void UpdateQuantityDisplay()
    {
        if (quantityText != null)
        {
            // Only show the quantity if it's greater than 1
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
    }

    public void AddToStack(int amount = 1)
    {
        quantity += amount;
        UpdateQuantityDisplay();
    }

    public int RemoveFromStack(int amount = 1)
    {
        int removed = Mathf.Min(amount, quantity);
        quantity -= removed;
        UpdateQuantityDisplay();
        return removed;
    }

    // This method creates a clone of the item suitable for the inventory UI slot.
    public GameObject CloneItem(int newQuantity)
    {
        GameObject clone = Instantiate(gameObject);
        Item cloneItem = clone.GetComponent<Item>();

        cloneItem.quantity = newQuantity;

        // --- ADD THIS LINE ---
        // By clearing the worldID, the WorldStateManager will ignore this icon
        cloneItem.worldID = "";

        cloneItem.UpdateQuantityDisplay();

        if (clone.TryGetComponent<WorldCollectible>(out WorldCollectible worldCollectible))
        {
            Destroy(worldCollectible);
        }

        return clone;
    }

    public virtual void UseItem()
    {
        Debug.Log("Using item " + Name);
    }

    public virtual void ShowPopUp()
    {
        // Improved: Try to find the icon from either SpriteRenderer (World Item) or Image (UI Item)
        Sprite itemIcon = null;

        if (TryGetComponent<SpriteRenderer>(out SpriteRenderer renderer))
        {
            itemIcon = renderer.sprite;
        }
        else if (TryGetComponent<Image>(out Image image))
        {
            itemIcon = image.sprite;
        }

        if (ItemPickupUIController.Instance != null && itemIcon != null)
        {
            ItemPickupUIController.Instance.ShowItemPickup(Name, itemIcon);
        }
    }
}