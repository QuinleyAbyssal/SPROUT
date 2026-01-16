using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [Header("Internal References")]
    private Slot[] inventorySlots;
    private ItemDictionary itemDictionary;
    public GameObject menuObject;      // The main "Menu" parent
    public GameObject pagesContainer;  // The "Pages" object

    [Header("UI References")]
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount = 20;

    public static InventoryController Instance { get; private set; }

    private Dictionary<int, int> itemsCountCache = new();
    public event Action OnInventoryChanged;
    private ItemData selectedGift;
    public Dictionary<int, int> GetItemCounts()
    {
        // Returns the cache we already calculate in RebuildItemCounts
        return itemsCountCache;
    }
    private ItemData currentlySelectedItem;

    // Call this when a player clicks an item slot in your UI
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Only DontDestroyOnLoad if this is the main GameController
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        itemDictionary = ItemDictionary.Instance;
        InitializeInventoryUI();
        RebuildItemCounts();
    }
    public void SelectItem(ItemData item)
    {
        currentlySelectedItem = item;
        Debug.Log("Selected item for gifting: " + (item != null ? item.itemName : "None"));
    }
    public void SetSelectedGift(ItemData item)
    {
        selectedGift = item;
    }

    public ItemData GetSelectedGift()
    {
        return selectedGift;
    }
    // This is what the NPC script calls
    public ItemData GetCurrentlySelectedItem()
    {
        return currentlySelectedItem;
    }
    public void ShowInventoryForGifting()
    {
        if (menuObject != null) menuObject.SetActive(true);
        if (pagesContainer != null) pagesContainer.SetActive(true);
        if (inventoryPanel != null) inventoryPanel.SetActive(true); // This is your InventoryPage
    }

    // --- INITIALIZATION ---
    public void InitializeInventoryUI()
    {
        // Safety: Ensure the panel exists
        if (inventoryPanel == null) return;

        // Generate slots if the panel is empty
        if (inventoryPanel.transform.childCount == 0 && slotPrefab != null)
        {
            for (int i = 0; i < slotCount; i++)
            {
                GameObject newSlot = Instantiate(slotPrefab, inventoryPanel.transform);
                newSlot.name = $"Slot_{i}";
            }
        }

        // Capture all slots (including inactive ones)
        inventorySlots = inventoryPanel.GetComponentsInChildren<Slot>(true);
    }

    // --- ITEM PICKUP & ADDITION ---
    public void HandleItemPickup(Item itemToCollect)
    {
        if (itemToCollect == null || !itemToCollect.enabled) return;

        if (AddItem(itemToCollect.gameObject))
        {
            itemToCollect.ShowPopUp();

            // FIX: Use the Item's worldID instead of searching for a second component
            if (!string.IsNullOrEmpty(itemToCollect.worldID))
            {
                WorldStateManager.Instance?.MarkCollected(itemToCollect.worldID);
            }

            Destroy(itemToCollect.gameObject);
        }
    }

    public bool AddItem(GameObject itemObject)
    {
        Item itemToAdd = itemObject.GetComponent<Item>();
        if (itemToAdd == null) return false;

        int quantityRemaining = itemToAdd.quantity;

        // 1. Try to stack with existing items first
        foreach (Slot slot in inventorySlots)
        {
            if (quantityRemaining <= 0) break;

            if (slot.currentItem != null)
            {
                Item slotItem = slot.currentItem.GetComponent<Item>();
                if (slotItem.ID == itemToAdd.ID && slotItem.MaxStackSize > 1)
                {
                    int space = slotItem.MaxStackSize - slotItem.quantity;
                    int toAdd = Mathf.Min(quantityRemaining, space);
                    slotItem.AddToStack(toAdd);
                    quantityRemaining -= toAdd;
                }
            }
        }

        // 2. Add to empty slots if quantity remains
        if (quantityRemaining > 0)
        {
            foreach (Slot slot in inventorySlots)
            {
                if (slot.currentItem == null)
                {
                    // Create a UI-friendly clone
                    GameObject newItem = itemToAdd.CloneItem(quantityRemaining);

                    // --- UI FORMATTING START ---
                    newItem.transform.SetParent(slot.transform);
                    newItem.transform.localScale = Vector3.one;
                    newItem.layer = LayerMask.NameToLayer("UI");

                    RectTransform rt = newItem.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchoredPosition = Vector2.zero;
                        // Adjust these values to match your slot size
                        rt.sizeDelta = new Vector2(50, 50);
                    }
                    // --- UI FORMATTING END ---

                    slot.currentItem = newItem;
                    quantityRemaining = 0;
                    break;
                }
            }
        }

        RebuildItemCounts();
        return quantityRemaining <= 0;
    }

    // --- UTILITIES & QUEST HELPERS ---
    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();
        if (inventorySlots == null) inventorySlots = inventoryPanel.GetComponentsInChildren<Slot>(true);

        foreach (Slot slot in inventorySlots)
        {
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    itemsCountCache[item.ID] = itemsCountCache.GetValueOrDefault(item.ID, 0) + item.quantity;
                }
            }
        }
        OnInventoryChanged?.Invoke();
    }
    public int GetTotalItemCount()
    {
        int total = 0;
        // Assuming you have a list or array of slots
        foreach (var slot in inventorySlots)
        {
            if (slot.HasItem()) total++;
        }
        return total;
    }
    public int GetItemCount(ItemData itemToCount)
    {
        if (itemToCount == null) return 0;
        return itemsCountCache.GetValueOrDefault(itemToCount.itemID, 0);
    }

    public void RemoveItem(ItemData itemToRemove, int amountToRemove)
    {
        int remainingToRemove = amountToRemove;

        for (int i = inventorySlots.Length - 1; i >= 0; i--)
        {
            if (remainingToRemove <= 0) break;

            Slot slot = inventorySlots[i];
            if (slot.currentItem != null)
            {
                Item itemScript = slot.currentItem.GetComponent<Item>();
                if (itemScript != null && itemScript.ID == itemToRemove.itemID)
                {
                    if (itemScript.quantity <= remainingToRemove)
                    {
                        remainingToRemove -= itemScript.quantity;
                        Destroy(slot.currentItem);
                        slot.currentItem = null;
                    }
                    else
                    {
                        itemScript.quantity -= remainingToRemove;
                        remainingToRemove = 0;
                    }
                }
            }
        }
        RebuildItemCounts();
    }

    /// <summary>
    /// Destroys all item GameObjects in the UI slots.
    /// </summary>
    public void ClearAllInventorySlots()
    {
        if (inventorySlots == null) return;

        foreach (Slot slot in inventorySlots)
        {
            if (slot.currentItem != null)
            {
                Destroy(slot.currentItem);
                slot.currentItem = null;
            }
        }
    }

    /// <summary>
    /// This is the method called by GameController. It wipes the UI and resets data.
    /// </summary>
    public void ClearInventory()
    {
        // Fixes image_6f84c6.png and image_6f813d.png
        ClearAllInventorySlots(); // Wipes the actual UI objects
        itemsCountCache.Clear();   // Resets the quest data
        OnInventoryChanged?.Invoke();
    }
    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].currentItem != null)
            {
                Item item = inventorySlots[i].currentItem.GetComponent<Item>();
                if (item != null)
                {
                    invData.Add(new InventorySaveData
                    {
                        itemID = item.ID,
                        slotIndex = i,
                        quantity = item.quantity
                    });
                }
            }
        }
        return invData;
    }
    // --- SAVE / LOAD LOGIC ---
    public void RefreshUI()
    {
        // This ensures the inventory panel is updated and item counts are synced for quests
        RebuildItemCounts();
        Debug.Log("Inventory UI Refreshed.");
    }
    public void SetInventoryItems(List<InventorySaveData> savedData)
    {
        ClearAllInventorySlots();
        InitializeInventoryUI();

        foreach (var data in savedData)
        {
            if (data.slotIndex < 0 || data.slotIndex >= inventorySlots.Length) continue;

            GameObject itemPrefab = ItemDictionary.Instance.GetItemPrefab(data.itemID);
            if (itemPrefab != null)
            {
                // 1. Instantiate the item
                GameObject newItemGo = Instantiate(itemPrefab);

                // 2. IMMEDIATELY configure its data before it can process its own Start()
                Item itemScript = newItemGo.GetComponent<Item>();
                itemScript.quantity = data.quantity;
                itemScript.worldID = ""; // Clear the ID so WorldStateManager ignores it

                // 3. FORCE IT TO BE ACTIVE (This overrides the Item.cs Start() logic)
                newItemGo.SetActive(true);
                newItemGo.layer = LayerMask.NameToLayer("UI");

                // 4. Parenting and UI Setup
                Slot targetSlot = inventorySlots[data.slotIndex];
                newItemGo.transform.SetParent(targetSlot.transform, false);

                RectTransform rt = newItemGo.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = Vector2.zero;
                    rt.localScale = Vector3.one;
                    rt.localPosition = Vector3.zero; // Ensure it's not behind the UI
                    rt.sizeDelta = new Vector2(50, 50);
                }

                targetSlot.currentItem = newItemGo;
            }
        }

        // 5. Finalize the state so the NPC sees the items
        RebuildItemCounts();
        Debug.Log("Inventory Loaded and Rebuilt.");
    }
}
