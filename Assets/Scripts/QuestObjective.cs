using System;
using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    // --- STATIC DEFINITION / SAVED PROGRESS DATA ---
    public ObjectiveType type;
    public string objectiveID;

    [Tooltip("The text displayed in the Quest UI (e.g., 'Collect 5 Red Flowers').")]
    public string description;

    [Tooltip("The total number of times this action must be performed.")]
    public int requiredAmount;

    // --- SAVED PROGRESS DATA ONLY ---
    public int currentAmount;
    public bool IsCompleted;

    // --- CONSTRUCTORS ---

    // 1. Required for JSON loading
    public QuestObjective() { }

    // 2. Used when starting a new quest to copy data from the ScriptableObject
    public QuestObjective(QuestObjective staticObjective)
    {
        this.type = staticObjective.type;
        this.objectiveID = staticObjective.objectiveID;
        this.description = staticObjective.description;
        this.requiredAmount = staticObjective.requiredAmount;
        this.currentAmount = 0;
        this.IsCompleted = false;
    }

    // --- HELPER METHODS ---
    public void IncreaseProgress(int amount = 1)
    {
        if (IsCompleted) return;

        currentAmount += amount;

        if (currentAmount >= requiredAmount)
        {
            currentAmount = requiredAmount;
            IsCompleted = true;
        }
    }
}