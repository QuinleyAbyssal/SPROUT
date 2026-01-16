using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    private string savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            savePath = Path.Combine(Application.persistentDataPath, "gamesave.json");
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // 1. Save Inventory (Fixes image_6f8bca.png)
        if (InventoryController.Instance != null)
            data.inventory = InventoryController.Instance.GetInventoryItems();

        // 2. Save Dialogue Progress (Fixes image_6e97c8.png)
        if (DialogueController.Instance != null)
            data.completedDialogues = new List<string>(DialogueController.Instance.completedDialogueIDs);

        // 3. Save World State (Collected Items)
        if (WorldStateManager.Instance != null)
            data.collectedItemIDs = WorldStateManager.Instance.GetCollectedIDs();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game Saved to: " + savePath);
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // 1. Load Inventory
        if (InventoryController.Instance != null)
            InventoryController.Instance.SetInventoryItems(data.inventory);

        // 2. Load Dialogue
        if (DialogueController.Instance != null)
            DialogueController.Instance.SetCompletedDialogues(data.completedDialogues);

        // 3. Load World State
        if (WorldStateManager.Instance != null)
            WorldStateManager.Instance.LoadCollectedStates(data.collectedItemIDs);

        Debug.Log("Game Loaded!");
    }
}
[System.Serializable]
public class FriendshipData
{
    public string npcName;
    public int points;
    public int level; // e.g., 0-10 hearts
    public List<string> unlockedRewards; // Track what has already been given
}
[System.Serializable]
public class FriendshipSaveData
{
    public string npcName;
    public int level;
}
[System.Serializable]
public class SaveData
{
    public List<InventorySaveData> inventory;
    public List<string> completedDialogues;
    public List<string> collectedItemIDs;
    public string currentMusicTrack;

    // Position and World Data
    public int sceneIndex; // Add this line to store the scene
    public Vector3 playerPosition;
    public List<string> collectedWorldItemIDs;
    public List<InventorySaveData> inventorySaveData;

    // Quest and Map Data
    public List<QuestProgress> questProgressData; // Fixed Type
    public List<string> handinQuestIDs;
    public string mapBoundary; // Fixed Type to accept GameObject names
    public List<FriendshipSaveData> friendshipLevels = new List<FriendshipSaveData>();
}