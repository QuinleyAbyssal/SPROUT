using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuestLogUI : MonoBehaviour
{
    public static QuestLogUI Instance;

    [Header("UI Prefabs & Containers")]
    [SerializeField] private GameObject questEntryPrefab; // A prefab with a Text component
    [SerializeField] private Transform contentContainer;  // The UI object with Vertical Layout Group

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // This is the method called by QuestController after loading data
    public void RedrawQuestList()
    {
        // 1. Clear existing UI entries to avoid duplicates
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Get the list of quests from the Controller
        var activeQuests = QuestController.Instance.activeQuestProgress;

        // 3. Create a new UI entry for every quest
        foreach (var progress in activeQuests)
        {
            if (progress.quest != null)
            {
                CreateQuestEntry(progress);
            }
            else
            {
                Debug.LogWarning("Found quest progress with missing ScriptableObject reference!");
            }
        }
    }

    private void CreateQuestEntry(QuestProgress progress)
    {
        GameObject entry = Instantiate(questEntryPrefab, contentContainer);

        if (entry.TryGetComponent<QuestEntryUI>(out QuestEntryUI ui))
        {
            // FIX FOR ERROR 1: 
            // If your Quest script doesn't have "QuestName", use "QuestID" or "name"
            string displayName = progress.quest.QuestID;

            ui.Setup(progress.GetCurrentObjectiveDescription());
        }
    }
}