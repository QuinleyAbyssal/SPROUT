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
}