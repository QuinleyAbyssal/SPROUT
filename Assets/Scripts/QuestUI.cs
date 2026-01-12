using UnityEngine;
using TMPro; // Required for TextMeshProUGUI and TMP_Text
using System.Collections.Generic;

public class QuestUI : MonoBehaviour
{
    // --- Serialized UI References (Must be assigned in Inspector) ---
    [Header("UI References")]
    [Tooltip("The parent Transform for all instantiated quest entries (e.g., Content of a Scroll View).")]
    public Transform questListContent;

    [Tooltip("The prefab for a single quest, containing the name and the objective list container.")]
    public GameObject questEntryPrefab;

    [Tooltip("The prefab for a single line of objective text. Must have a direct TMP_Text component.")]
    public GameObject objectiveTextPrefab;

    // --- System Dependency ---
    // We get a local reference to the Singleton QuestController.
    private QuestController questController;

    // --- Initialization and Loading Safety ---

    private void Awake()
    {
        // Awake is typically too early to guarantee Singleton dependencies are ready.
        // We defer dependency lookup until OnEnable/UpdateUI for maximum safety.
    }

    private void OnEnable()
    {
        // Try to find the instance here to catch it if it initialized first.
        if (questController == null)
        {
            questController = QuestController.Instance;
        }
    }

    void Start()
    {
        // Final dependency check before initial update
        if (questController == null)
        {
            questController = QuestController.Instance;
        }

        if (questController != null)
        {
            UpdateQuestUI();
        }
    }

    /// <summary>
    /// Refreshes the display based on the current state of the QuestController.
    /// This is the method called by QuestController upon acceptance, progress, and loading.
    /// </summary>
    public void UpdateQuestUI()
    {
        // 🛑 CRITICAL SAFETY CHECK 1: Ensure QuestController is available
        if (questController == null)
        {
            questController = QuestController.Instance;
        }

        if (questController == null)
        {
            Debug.LogError("QuestController is NOT INITIALIZED. Cannot update UI. Check Script Execution Order.");
            return;
        }

        // --- 1. Destroy existing quest entries ---
        if (questListContent == null)
        {
            Debug.LogError("Quest List Content Transform is not assigned in the Inspector!");
            return;
        }

        // Clear previous entries
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        // --- 2. Build quest entries ---

        // CRITICAL CHECK 2: Ensure required prefabs are assigned
        if (questEntryPrefab == null || objectiveTextPrefab == null)
        {
            Debug.LogError("Quest Entry or Objective Text Prefab is not assigned in the Inspector.");
            return;
        }

        foreach (var quest in questController.activeQuestProgress)
        {
            // Instantiate the main quest entry
            GameObject entry = Instantiate(questEntryPrefab, questListContent);

            // --- 2.1 Quest Name Display ---
            // Use Find() to get the child element (Null-conditional operator prevents crash if not found)
            TMP_Text questNameText = entry.transform.Find("QuestNameText")?.GetComponent<TMP_Text>();

            // Get the objectives container Transform
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            if (questNameText != null)
            {
                // Ensure the transient Quest ScriptableObject reference is restored
                if (quest.quest != null)
                {
                    questNameText.text = quest.quest.questName;
                }
                else
                {
                    questNameText.text = $"[Quest ID: {quest.QuestID}] - ERROR LOADING";
                }
            }

            // 🛑 CRITICAL FIX for NRE at line ~112: Check if the objectives container was found
            if (objectiveList == null)
            {
                Debug.LogError($"DIAGNOSTIC FAILED: objectiveList is NULL. Check the spelling of 'ObjectiveList' inside the prefab '{questEntryPrefab.name}'!");
                continue;
            }
            if (objectiveTextPrefab == null)
            {
                Debug.LogError("DIAGNOSTIC FAILED: objectiveTextPrefab is NULL. Check Inspector assignment!");
                continue;
            }

            // --- 2.2 Objective List Display ---

            // 🛑 CRITICAL FIX for runtime error: Check if the Objectives list itself is null (Corrupted Save Data)
            if (quest.Objectives == null)
            {
                Debug.LogError($"QUEST DATA CORRUPTED: Quest ID {quest.QuestID} has a null Objectives list. Skipping UI generation for this quest.");
                continue;
            }

            foreach (var objective in quest.Objectives)
            {
                // ULTIMATE SAFETY: Wrap the instantiation process in a try-catch to prevent an abrupt crash.
                try
                {
                    // Line 112: Instantiation (where the crash likely occurs)
                    GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);

                    // Get the TMP_Text component (using recursive search, which is safer)
                    TMP_Text objText = objTextGO.GetComponentInChildren<TMP_Text>(true);

                    if (objText == null)
                    {
                        Debug.LogError($"FINAL CRASH DIAGNOSIS: Instantiated objective line prefab '{objTextGO.name}' does NOT contain a TMP_Text component anywhere in its hierarchy. Check your prefab structure!");
                        continue;
                    }

                    // Use rich text for color coding
                    string color = objective.IsCompleted ? "#00FF00" : "#FFFFFF"; // Green or White

                    objText.text = $"<color={color}>{objective.description} ({objective.currentAmount}/{objective.requiredAmount})</color>";

                }
                catch (System.Exception ex)
                {
                    // If the crash is due to a faulty prefab or a corrupted object reference that Instantiate can't handle.
                    Debug.LogError($"CRITICAL INSTANTIATION FAILURE for Quest ID {quest.QuestID}: {ex.Message}. Check your '{objectiveTextPrefab.name}' asset.");
                    // Stop processing objectives for this quest entry.
                    break;
                }
            }
        }
    }
}