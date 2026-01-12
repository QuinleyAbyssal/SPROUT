using System.Collections;
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("Data")]
    public NPCDialogue dialogueData;

    // --- State Variables ---
    private int dialogueIndex;
    private enum QuestState { NotStarted, InProgress, Completed, HandedIn }
    private QuestState questState = QuestState.NotStarted;
    private bool isDialogueActive = false;

    private void Start()
    {
        // Initialization handled via DialogueController.Instance
    }

    public bool CanInteract()
    {
        // Allow interaction if no dialogue is active OR if the current dialogue belongs to this NPC
        return !PauseController.IsGamePaused || isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive))
            return;

        if (isDialogueActive)
        {
            // If the controller is still typing the letters
            if (DialogueController.Instance.IsTyping)
            {
                DialogueController.Instance.SkipTyping();
                CheckForChoices(); // Immediately show choices if the player skips typing
            }
            // If choices are currently on screen, do nothing (wait for button click)
            else if (DialogueController.Instance.ChoiceActive())
            {
                return;
            }
            // Otherwise, move to the next line
            else
            {
                NextLine();
            }
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        SyncQuestState();

        // Determine starting index based on Quest progress
        if (questState == QuestState.NotStarted) dialogueIndex = dialogueData.DefaultStartIndex;
        else if (questState == QuestState.InProgress) dialogueIndex = dialogueData.questInProgressIndex;
        else if (questState == QuestState.Completed) dialogueIndex = dialogueData.questCompletedIndex;
        else if (questState == QuestState.HandedIn) dialogueIndex = dialogueData.postQuestIndex;

        isDialogueActive = true;
        DialogueController.Instance.StartDialogue(dialogueData, dialogueIndex);
        PauseController.SetPause(true);

        StartCoroutine(WaitForTypeToEnd());
    }

    private IEnumerator WaitForTypeToEnd()
    {
        // Wait until the DialogueController finishes the typewriter effect
        while (DialogueController.Instance.IsTyping)
        {
            yield return null;
        }
        CheckForChoices();
    }

    void CheckForChoices()
    {
        DialogueController.Instance.ClearChoices();

        foreach (DialogueChoice choice in dialogueData.choices)
        {
            if (choice.dialogueIndex == dialogueIndex)
            {
                DisplayChoices(choice);
                break;
            }
        }
    }

    void DisplayChoices(DialogueChoice choice)
    {
        for (int i = 0; i < choice.choices.Length; i++)
        {
            int nextIdx = choice.nextDialogueIndexes[i];
            bool givesQuest = i < choice.givesQuest.Length && choice.givesQuest[i];

            DialogueController.Instance.CreateChoiceButton(choice.choices[i], () => ChooseOption(nextIdx, givesQuest));
        }
    }

    void ChooseOption(int nextIndex, bool givesQuest)
    {
        if (givesQuest && dialogueData.quest != null)
        {
            QuestController.Instance.AcceptQuest(dialogueData.quest);
        }

        dialogueIndex = nextIndex;
        DialogueController.Instance.ClearChoices();
        DialogueController.Instance.UpdateLine(dialogueIndex);

        StartCoroutine(WaitForTypeToEnd());
    }

    void NextLine()
    {
        // Check if the current line is marked as an "End" line in the ScriptableObject
        if (dialogueIndex < dialogueData.endDialogueLines.Length && dialogueData.endDialogueLines[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        // Advance index or end if out of lines
        if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            DialogueController.Instance.UpdateLine(dialogueIndex);
            StartCoroutine(WaitForTypeToEnd());
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        // Handle Quest Completion/Rewards if conditions are met
        if (questState == QuestState.Completed && dialogueData.quest != null)
        {
            if (!QuestController.Instance.IsQuestHandedIn(dialogueData.quest.QuestID))
            {
                HandleQuestCompletion(dialogueData.quest);
            }
        }

        isDialogueActive = false;
        DialogueController.Instance.EndDialogue();
        PauseController.SetPause(false);
        StopAllCoroutines();
    }

    public void SyncQuestState()
    {
        if (dialogueData.quest == null) return;

        string id = dialogueData.quest.QuestID;

        // 1. Check if already handed in
        if (QuestController.Instance.IsQuestHandedIn(id))
        {
            questState = QuestState.HandedIn;
        }
        // 2. Check if the quest is active
        else if (QuestController.Instance.IsQuestActive(id))
        {
            // NEW: Only set to Completed if the player actually HAS the items
            if (PlayerHasAllRequirements(dialogueData.quest))
            {
                questState = QuestState.Completed;
            }
            else
            {
                questState = QuestState.InProgress;
            }
        }
        else
        {
            questState = QuestState.NotStarted;
        }
    }

    // Helper method to verify items
    private bool PlayerHasAllRequirements(Quest quest)
    {
        // Ensure QuestController has a way to check item requirements
        // This assumes your Quest object has a list of required items/quantities
        return QuestController.Instance.AreQuestRequirementsMet(quest);
    }

    void HandleQuestCompletion(Quest questToHandIn)
    {
        // 1. Remove required items from inventory
        foreach (var requirement in questToHandIn.requiredItems)
        {
            InventoryController.Instance.RemoveItem(requirement.itemData, requirement.amount);
        }

        // 2. Grant Rewards
        RewardsController.Instance.GrantRewards(questToHandIn);

        // 3. Mark as Handed In
        QuestController.Instance.HandInQuest(questToHandIn.QuestID);

        questState = QuestState.HandedIn;
    }
}