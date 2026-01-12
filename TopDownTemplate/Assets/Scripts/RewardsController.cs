using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardsController : MonoBehaviour
{
    public static RewardsController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GrantRewards(Quest quest)
    {
        // 1. Safety check to ensure the quest asset exists
        if (quest == null || quest.questRewards == null) return;

        foreach (var reward in quest.questRewards)
        {
            switch (reward.type)
            {
                case RewardType.Item:
                    // 2. Pass the ItemData asset and the amount
                    GiveItemReward(reward.itemReward, reward.amount);
                    break;
                case RewardType.Gold:
                    // Logic: CurrencyManager.Instance.AddGold(reward.amount);
                    break;
                case RewardType.Experience:
                    // Logic: PlayerStats.Instance.AddXP(reward.amount);
                    break;
                case RewardType.Custom:
                    // Logic: Handle unique rewards like unlocking a new area
                    break;
            }
        }
    }

    private void GiveItemReward(ItemData data, int amount)
    {
        // 3. Verify ItemData and Dictionary exist
        if (data == null || ItemDictionary.Instance == null)
        {
            Debug.LogError("Cannot give reward: ItemData or ItemDictionary is missing.");
            return;
        }

        // 4. Use your ItemDictionary to get the prefab linked to this ItemData
        var itemPrefab = ItemDictionary.Instance.GetPrefabByData(data);

        if (itemPrefab == null)
        {
            Debug.LogError($"Item prefab not found for: {data.itemName}. Check your ItemDictionary list.");
            return;
        }

        // 5. Add items to inventory
        for (int i = 0; i < amount; i++)
        {
            if (InventoryController.Instance == null) return;

            if (!InventoryController.Instance.AddItem(itemPrefab))
            {
                // 6. Full Inventory: Spawn the item on the ground near the player/NPC
                GameObject dropItem = Instantiate(itemPrefab, transform.position + Vector3.up, Quaternion.identity);

                // Set the quantity on the dropped item's script
                if (dropItem.TryGetComponent<Item>(out Item itemScript))
                {
                    itemScript.quantity = 1;
                }

                dropItem.GetComponent<BounceEffect>()?.StartBounce();
                Debug.LogWarning($"Inventory full. Dropped {data.itemName} at {transform.position}");
            }
            else
            {
                // 7. Success: Trigger the pop-up notification
                // We get the component from the prefab's script for the name/icon data
                itemPrefab.GetComponent<Item>()?.ShowPopUp();
            }
        }
    }
}