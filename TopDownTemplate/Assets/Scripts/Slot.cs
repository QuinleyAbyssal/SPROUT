using UnityEngine;

public class Slot : MonoBehaviour
{
    [Tooltip("The item currently childed to this slot UI")]
    public GameObject currentItem;

    private void Update()
    {
        // Safety Check: If the item was destroyed elsewhere but the slot 
        // still thinks it has it, clear the reference.
        if (currentItem == null)
        {
            currentItem = null;
        }
    }
    public bool HasItem()
    {
        // Return true if there is an item object currently assigned to this slot
        return currentItem != null;
    }

    public void OnSlotClicked()
    {
        if (currentItem == null) return;
        Item itemScript = currentItem.GetComponent<Item>();

        if (itemScript != null)
        {
            InventoryController.Instance.SetSelectedGift(itemScript.itemData);

            // Instead of searching the whole scene, ask the controller who is talking
            NPC currentNPC = DialogueController.Instance.currentSpeaker;

            if (currentNPC != null)
            {
                Debug.Log("Found Speaker: " + currentNPC.name + " | Waiting: " + currentNPC.IsWaitingForGift());

                if (currentNPC.IsWaitingForGift())
                {
                    currentNPC.ReceiveGift(itemScript.itemData);
                    InventoryController.Instance.menuObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("No Current Speaker found in DialogueController!");
            }
        }
    }
}