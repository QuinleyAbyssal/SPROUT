using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    [Header("Identification")]
    public string QuestID;
    public string questName;
    [TextArea(3, 10)] public string description;

    [Header("Objectives")]
    // Used for the Quest Log UI to track progress (Kill 10 wolves, etc.)
    public List<QuestObjective> Objectives;

    [Header("Requirements")]
    // NEW: Used by the NPC and QuestController to check inventory before finishing
    public List<QuestRequirement> requiredItems;

    [Header("Rewards")]
    public List<QuestReward> questRewards;

    private void OnValidate()
    {
        // Auto-generate ID if empty to prevent save-data conflicts
        if (string.IsNullOrEmpty(QuestID) && !string.IsNullOrEmpty(questName))
        {
            QuestID = questName.Replace(" ", "_") + "_" + Guid.NewGuid().ToString().Substring(0, 8);
        }
    }
}

[Serializable]
public class QuestRequirement
{
    public ItemData itemData;
    public int amount;
}