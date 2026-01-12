using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Identification")]
    public string itemName;
    
    [Tooltip("This MUST match the 'ID' integer set on the Item.cs script on your prefab.")]
    public int itemID;

    [Header("Visuals & Spawning")]
    public Sprite icon;
    
    [Tooltip("The actual prefab that has the Item.cs script attached.")]
    public GameObject itemPrefab;

    [Header("Description")]
    [TextArea(3, 10)]
    public string description;
}