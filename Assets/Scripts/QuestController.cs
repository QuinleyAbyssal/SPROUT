using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }

    [Header("Quest Lists")]
    // This is the list the SaveLoadManager will look for
    public List<QuestProgress> activeQuestProgress = new List<QuestProgress>();

    // IDs of quests already finished and rewarded
    public List<string> handinQuestIDs = new List<string>();

    private QuestUI questUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        questUI = FindObjectOfType<QuestUI>();
    }

    private void Start()
    {
        // Listen for inventory changes to update quest progress automatically
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.OnInventoryChanged += CheckInventoryForQuests;
        }
    }
    public void RefreshQuestProgress()
    {
        CheckInventoryForQuests();
        Debug.Log("DEBUG: Quest progress manually refreshed.");
    }
    // --- SAVE/LOAD INTERFACE ---

    // This is what the SaveLoadManager calls
    public List<QuestProgress> GetSerializedProgress()
    {
        return activeQuestProgress;
    }

    public void LoadQuestProgress(List<QuestProgress> loadedProgress, List<string> handinIDs)
    {
        activeQuestProgress.Clear();
        handinQuestIDs = handinIDs ?? new List<string>();

        if (loadedProgress == null) return;

        foreach (QuestProgress progress in loadedProgress)
        {
            // Re-link the ScriptableObject asset based on the ID
            Quest questAsset = QuestDictionary.Instance?.GetQuestByID(progress.QuestID);

            if (questAsset != null)
            {
                progress.quest = questAsset;
                activeQuestProgress.Add(progress);
            }
            else
            {
                Debug.LogError($"Quest ID {progress.QuestID} not found in Dictionary!");
            }
        }

        questUI?.UpdateQuestUI();
    }

    // --- QUEST LOGIC ---

    public void AcceptQuest(Quest quest)
    {
        if (IsQuestActive(quest.QuestID) || IsQuestHandedIn(quest.QuestID)) return;

        activeQuestProgress.Add(new QuestProgress(quest));
        CheckInventoryForQuests();
        questUI?.UpdateQuestUI();
    }

    public bool IsQuestActive(string questID) => activeQuestProgress.Exists(q => q.QuestID == questID);
    public bool IsQuestHandedIn(string questID) => handinQuestIDs.Contains(questID);

    public void CheckInventoryForQuests()
    {
        if (InventoryController.Instance == null) return;

        Dictionary<int, int> itemCounts = InventoryController.Instance.GetItemCounts();
        bool uiUpdated = false;

        foreach (QuestProgress quest in activeQuestProgress)
        {
            if (quest.IsCompleted()) continue;

            foreach (QuestObjective objective in quest.Objectives)
            {
                if (objective.type != ObjectiveType.CollectItem || objective.IsCompleted) continue;

                // Try to get Item ID from the objective string
                if (int.TryParse(objective.objectiveID, out int itemID))
                {
                    int currentCount = itemCounts.TryGetValue(itemID, out int count) ? count : 0;
                    int newAmount = Mathf.Min(currentCount, objective.requiredAmount);

                    if (objective.currentAmount != newAmount)
                    {
                        objective.currentAmount = newAmount;
                        objective.IsCompleted = (newAmount >= objective.requiredAmount);
                        uiUpdated = true;
                    }
                }
            }
        }

        if (uiUpdated) questUI?.UpdateQuestUI();
    }
    public bool AreQuestRequirementsMet(Quest quest)
    {
        if (InventoryController.Instance == null) return false;

        // Check every item required by the ScriptableObject
        foreach (var requirement in quest.requiredItems)
        {
            // Make sure your InventoryController has a GetItemCount function!
            int playerAmount = InventoryController.Instance.GetItemCount(requirement.itemData);
            if (playerAmount < requirement.amount) return false;
        }
        return true;
    }
    public void HandInQuest(string questID)
    {
        QuestProgress progress = activeQuestProgress.Find(q => q.QuestID == questID);
        if (progress == null || !progress.IsCompleted()) return;

        // Remove items from inventory
        foreach (var requirement in progress.quest.requiredItems)
        {
            InventoryController.Instance.RemoveItem(requirement.itemData, requirement.amount);
        }

        // Grant rewards
        RewardsController.Instance?.GrantRewards(progress.quest);

        // Move to handed-in list
        handinQuestIDs.Add(questID);
        activeQuestProgress.Remove(progress);

        questUI?.UpdateQuestUI();
    }
}