using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestProgress
{
    // --- SAVED DATA ---
    public string QuestID;
    public List<QuestObjective> Objectives = new List<QuestObjective>();

    // --- TRANSIENT DATA ---
    [System.NonSerialized]
    public Quest quest;

    public QuestProgress() { }

    public QuestProgress(Quest questAsset)
    {
        this.QuestID = questAsset.QuestID;
        this.quest = questAsset;
        this.Objectives = new List<QuestObjective>();

        foreach (var obj in questAsset.Objectives)
        {
            this.Objectives.Add(new QuestObjective(obj));
        }
    }

    // --- FIX: Updated variable name to 'description' ---
    public string GetCurrentObjectiveDescription()
    {
        foreach (var obj in Objectives)
        {
            if (!obj.IsCompleted)
            {
                // Must match the 'description' field in QuestObjective.cs
                return obj.description;
            }
        }
        return "Quest Complete!";
    }

    public bool IsCompleted()
    {
        foreach (var objective in Objectives)
        {
            if (!objective.IsCompleted) return false;
        }
        return true;
    }
}