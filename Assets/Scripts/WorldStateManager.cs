using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class WorldStateManager : MonoBehaviour
{
    public static WorldStateManager Instance { get; private set; }

    [SerializeField]
    private List<string> collectedItemIDs = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Make sure this manager persists across scenes
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Tell Unity to run 'OnSceneLoaded' every time a scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If we are in the Menu scene, we don't need to sync world items
        if (scene.name == "MenuScene") return;

        // Otherwise, hide the items that were already collected
        SyncWorldState();
    }
    // --- NEW METHOD: THIS FIXES THE ERROR IN ITEM.CS ---
    public bool IsItemCollected(string uniqueID)
    {
        if (string.IsNullOrEmpty(uniqueID)) return false;
        return collectedItemIDs.Contains(uniqueID);
    }

    public void MarkCollected(string uniqueID)
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            Debug.LogError("Attempted to mark an item as collected with a null or empty ID.");
            return;
        }

        if (!collectedItemIDs.Contains(uniqueID))
        {
            collectedItemIDs.Add(uniqueID);
            Debug.Log($"WorldStateManager: Marked item ID '{uniqueID}' as collected.");
        }
    }

    // --- SAVE/LOAD INTERFACE ---

    public List<string> GetCollectedIDs()
    {
        return collectedItemIDs;
    }

    public void LoadCollectedStates(List<string> savedIDs)
    {
        collectedItemIDs = savedIDs ?? new List<string>();
        Debug.Log($"WorldStateManager: Loaded {collectedItemIDs.Count} collected item IDs.");

        // After loading the list, we must refresh the scene objects
        SyncWorldState();
    }

    // --- LOGIC: WORLD SYNCHRONIZATION ---

    public void SyncWorldState()
    {
        // This finds all items currently in the scene
        Item[] allItems = FindObjectsOfType<Item>();
        int removedCount = 0;

        foreach (Item item in allItems)
        {
            // Note: Ensure your Item.cs has a public field named 'worldID'
            if (collectedItemIDs.Contains(item.worldID))
            {
                item.gameObject.SetActive(false);
                removedCount++;
            }
        }

        Debug.Log($"WorldStateManager: Sync complete. Deactivated {removedCount} items.");
    }

}