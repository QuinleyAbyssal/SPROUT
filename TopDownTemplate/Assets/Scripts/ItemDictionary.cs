using UnityEngine;
using System.Collections.Generic;

public class ItemDictionary : MonoBehaviour
{
    public static ItemDictionary Instance { get; private set; }

    [Header("Item Data")]
    public GameObject[] itemPrefabs;

    private Dictionary<string, int> nameToIdMap = new Dictionary<string, int>();
    private Dictionary<int, GameObject> idToPrefabMap = new Dictionary<int, GameObject>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        // Initialize maps immediately in Awake so other scripts can access them in Start
        BuildNameIdMap();
    }

    private void BuildNameIdMap()
    {
        nameToIdMap.Clear();
        idToPrefabMap.Clear();

        foreach (GameObject prefab in itemPrefabs)
        {
            if (prefab == null) continue;

            if (prefab.TryGetComponent<Item>(out Item item))
            {
                // Map Name to ID
                if (!nameToIdMap.ContainsKey(item.Name))
                    nameToIdMap.Add(item.Name, item.ID);

                // Map ID to Prefab
                if (!idToPrefabMap.ContainsKey(item.ID))
                    idToPrefabMap.Add(item.ID, prefab);
                else
                    Debug.LogWarning($"Duplicate Item ID found: {item.ID} on {prefab.name}");
            }
        }
    }

    // --- Public Lookup Methods ---

    public int GetIdByName(string itemName)
    {
        if (nameToIdMap.TryGetValue(itemName, out int id)) return id;
        return 0;
    }

    public GameObject GetItemPrefab(int itemId)
    {
        if (idToPrefabMap.TryGetValue(itemId, out GameObject prefab))
        {
            return prefab;
        }
        return null;
    }

    /// <summary>
    /// NEW: Allows RewardsController to get the prefab directly from an ItemData asset.
    /// </summary>
    public GameObject GetPrefabByData(ItemData data)
    {
        if (data == null) return null;
        return GetItemPrefab(data.itemID);
    }
}