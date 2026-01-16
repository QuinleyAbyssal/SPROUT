using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FriendshipManager : MonoBehaviour
{
    public static FriendshipManager Instance;
    public int maxHearts = 5;
    public ItemData chirpLevel1Gift; // Drag the "Feather" here in Inspector
    public ItemData fishsticksLevel2Gift;
    public ItemData chirpLevel3Gift;
    public ItemData fishsticksLevel3Gift;

    public List<FriendshipData> npcFriendships = new List<FriendshipData>();
    [Header("Progression Settings")]
    // Define the points needed for each heart level
    // Element 0 = Heart 1, Element 1 = Heart 2, etc.
    public int[] heartThresholds = { 100, 300, 600, 1000, 1500 };

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // This is called by NPC.cs when a gift is received
    public void ReceiveGift(NPCDialogue npcData, ItemData gift)
    {
        if (npcData == null || gift == null) return;

        int pointsToAdd = 0;
        string itemName = gift.itemName;

        // Check preference lists in the ScriptableObject
        if (npcData.lovedItems.Contains(gift))
        {
            pointsToAdd = 50;
            Debug.Log($"{npcData.npcName} LOVED the {itemName}!");
        }
        else if (npcData.likedItems.Contains(gift))
        {
            pointsToAdd = 20;
            Debug.Log($"{npcData.npcName} liked the {itemName}.");
        }
        else if (npcData.hatedItems.Contains(gift))
        {
            pointsToAdd = -20;
            Debug.Log($"{npcData.npcName} hated the {itemName}!");
        }
        else
        {
            pointsToAdd = 10; // Neutral gift
            Debug.Log($"{npcData.npcName} was neutral about the {itemName}.");
        }

        AddPoints(npcData.npcName, pointsToAdd);
    }
    public List<FriendshipSaveData> GetSerializedFriendshipData()
    {
        List<FriendshipSaveData> data = new List<FriendshipSaveData>();
        // CHANGE THIS: Use npcFriendships instead of npcFriendshipLevels
        foreach (var entry in npcFriendships)
        {
            data.Add(new FriendshipSaveData { npcName = entry.npcName, level = entry.level });
        }
        return data;
    }

    public void LoadFriendshipData(List<FriendshipSaveData> savedData)
    {
        // CHANGE THIS: Use npcFriendships instead of npcFriendshipLevels
        npcFriendships.Clear();
        foreach (var item in savedData)
        {
            npcFriendships.Add(new FriendshipData { npcName = item.npcName, level = item.level, points = 0 });
        }
        Debug.Log("Friendship Data Loaded!");
    }
    public void AddPoints(string npcName, int amount)
    {
        if (npcName == "Fritter") return;

        FriendshipData data = npcFriendships.Find(n => n.npcName == npcName);
        if (data == null)
        {
            data = new FriendshipData { npcName = npcName, points = 0, level = 0 };
            npcFriendships.Add(data);
        }

        data.points += amount;

        // Cap points AFTER adding so thresholds can be met
        int maxPoints = heartThresholds[heartThresholds.Length - 1];
        data.points = Mathf.Clamp(data.points, 0, maxPoints);

        CheckForLevelUp(data);
    }

    private void CheckForLevelUp(FriendshipData data)
    {
        int reachedLevel = 0;
        for (int i = 0; i < heartThresholds.Length; i++)
        {
            if (data.points >= heartThresholds[i]) reachedLevel = i + 1;
            else break;
        }

        if (reachedLevel > data.level)
        {
            data.level = reachedLevel;

            // --- ADD THIS ACHIEVEMENT LOGIC ---
            if (data.level == 1)
            {
                // This triggers the achievement only the first time anyone hits level 1
                AchievementManager.Instance.UnlockAchievement("First steps");
            }
            // ----------------------------------

            UnlockRewards(data);

            FriendshipUI ui = FindObjectOfType<FriendshipUI>();
            if (ui != null) ui.UpdateHeartDisplay(data.npcName);
        }
    }
    public float GetProgressToNextHeart(string npcName)
    {
        FriendshipData data = npcFriendships.Find(n => n.npcName == npcName);
        if (data == null || data.level >= heartThresholds.Length) return 1f;

        int currentLevel = data.level;
        int prevThreshold = (currentLevel == 0) ? 0 : heartThresholds[currentLevel - 1];
        int nextThreshold = heartThresholds[currentLevel];

        float progress = (float)(data.points - prevThreshold) / (nextThreshold - prevThreshold);
        return Mathf.Clamp01(progress);
    }

    private void UnlockRewards(FriendshipData data)
    {
        // The switch handles which NPC is leveling up
        switch (data.npcName)
        {
            case "Chirp":
                if (data.level == 1)
                {
                    // Give Item at Level 1
                    InventoryController.Instance.AddItem(chirpLevel1Gift.itemPrefab);
                    Debug.Log("Chirp reached Heart 1: Gift Given!");
                }
                else if (data.level == 2)
                {
                    // Unlock Scene/Location at Level 2
                    // Example: SceneManager.LoadScene("NewSecretArea"); 
                    Debug.Log("Chirp reached Heart 2: New Location Unlocked!");
                }
                else if (data.level == 3)
                {
                    // Another Gift at Level 3
                    InventoryController.Instance.AddItem(chirpLevel3Gift.itemPrefab);
                    Debug.Log("Chirp reached Heart 3: Second Gift Given!");
                }
                if (data.level == 5)
                {
                    AchievementManager.Instance.UnlockAchievement("Best Friends with " + data.npcName);
                }
                break;

            case "Fishsticks":
                // Level 1: Nothing happens (Empty)
                if (data.level == 2)
                {
                    // Item at Level 2
                    InventoryController.Instance.AddItem(fishsticksLevel2Gift.itemPrefab);
                    Debug.Log("Fishsticks reached Heart 2: First Gift Given!");
                }
                else if (data.level == 3)
                {
                    // Another Item at Level 3
                    InventoryController.Instance.AddItem(fishsticksLevel3Gift.itemPrefab);
                    Debug.Log("Fishsticks reached Heart 3: Second Gift Given!");
                }
                break;
        }
    }

    public int GetLevel(string npcName)
    {
        FriendshipData data = npcFriendships.Find(n => n.npcName == npcName);
        return (data != null) ? data.level : 0;
    }

    public int GetPoints(string npcName)
    {
        FriendshipData data = npcFriendships.Find(n => n.npcName == npcName);
        return (data != null) ? data.points : 0;
    }

    [System.Serializable]
    public class FriendshipData
    {
        public string npcName;
        public int points;
        public int level; // This tracks how many hearts are currently unlocked
    }
}