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
    private bool waitingForGift = false;
    public bool IsWaitingForGift() => waitingForGift && isDialogueActive;

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
        // 1. Safety Check
        if (dialogueData == null) return;

        // 2. THE LOCK: If choosing a gift, do absolutely nothing.
        // This stops the dialogue from advancing when you click the UI.
        if (waitingForGift)
        {
            return;
        }

        // 3. Pause Check (Prevents interacting with others while paused)
        if (PauseController.IsGamePaused && !isDialogueActive)
            return;

        if (isDialogueActive)
        {
            if (DialogueController.Instance.IsTyping)
            {
                DialogueController.Instance.SkipTyping();
                CheckForChoices();
            }
            else if (DialogueController.Instance.ChoiceActive())
            {
                return; // Wait for the player to click a Choice Button
            }
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
    public void OnGiftChoiceSelected()
    {
        // CRITICAL: Stop the background timer so it doesn't trigger NextLine() automatically
        StopAllCoroutines();

        waitingForGift = true;
        DialogueController.Instance.ClearChoices();
        InventoryController.Instance.ShowInventoryForGifting();
    }
    public void ReceiveGift(ItemData gift)
    {
        Debug.Log("NPC received: " + gift.itemName);
        waitingForGift = false;

        // Remove item and add points...
        InventoryController.Instance.RemoveItem(gift, 1);
        FriendshipManager.Instance.ReceiveGift(dialogueData, gift);

        // DYNAMIC JUMP: Use the index specifically set for this NPC
        dialogueIndex = dialogueData.thankYouDialogueIndex;

        if (DialogueController.Instance != null)
        {
            DialogueController.Instance.UpdateLine(dialogueIndex);
            StartCoroutine(WaitForTypeToEnd());
        }
    }
    void StartDialogue()
    {
        // 1. Set current speaker
        if (DialogueController.Instance != null)
        {
            DialogueController.Instance.currentSpeaker = this;
        }

        SyncQuestState();

        // Determine starting index based on Quest progress
        if (questState == QuestState.NotStarted) dialogueIndex = dialogueData.DefaultStartIndex;
        else if (questState == QuestState.InProgress) dialogueIndex = dialogueData.questInProgressIndex;
        else if (questState == QuestState.Completed) dialogueIndex = dialogueData.questCompletedIndex;
        else if (questState == QuestState.HandedIn) dialogueIndex = dialogueData.postQuestIndex;

        isDialogueActive = true;

        // 3. FORCE THE HEARTS ON NOW
        // We find the UI and tell it to show hearts BEFORE starting the dialogue UI
        FriendshipUI ui = FindObjectOfType<FriendshipUI>();
        if (ui != null)
        {
            // If dialogueData.npcName is "Fritter", the UI code above will hide the hearts
            ui.UpdateHeartDisplay(dialogueData.npcName);
        }

        // 4. Start the UI display
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
        // Safety check for array sizing
        if (choice.requiredFriendshipLevel == null || choice.requiredFriendshipLevel.Length < choice.choices.Length)
        {
            choice.requiredFriendshipLevel = new int[choice.choices.Length];
        }

        for (int i = 0; i < choice.choices.Length; i++)
        {
            string choiceText = choice.choices[i];
            int currentLevel = 0;
            if (dialogueData.npcName != "Fritter")
            {
                currentLevel = FriendshipManager.Instance.GetLevel(dialogueData.npcName);
            }

            bool isGiftingOption = choiceText == "Give Gift" || choiceText == "Here's a gift for you!";

            // CHECK: Does the player have items?
            bool hasItemsToGive = InventoryController.Instance.GetTotalItemCount() > 0;

            // Logic for showing the button
            bool hasQuest = dialogueData.quest != null;
            bool questFinished = questState == QuestState.Completed || questState == QuestState.HandedIn;
            bool canGift = (!hasQuest || questFinished) && hasItemsToGive; // Added item check here

            if (currentLevel >= choice.requiredFriendshipLevel[i])
            {
                if (isGiftingOption && !canGift) continue;

                int nextIdx = choice.nextDialogueIndexes[i];
                bool givesQuest = i < choice.givesQuest.Length && choice.givesQuest[i];

                if (isGiftingOption)
                {
                    DialogueController.Instance.CreateChoiceButton(choiceText, () => OnGiftChoiceSelected());
                }
                else
                {
                    DialogueController.Instance.CreateChoiceButton(choiceText, () => ChooseOption(nextIdx, givesQuest));
                }
            }
        }
    }
    public void StartCutsceneDialogue(NPCDialogue cutsceneData, int startIndex = 0)
    {
        // 1. Temporarily swap the data
        NPCDialogue originalData = this.dialogueData;
        this.dialogueData = cutsceneData;

        // 2. Set the index
        this.dialogueIndex = startIndex;

        // 3. Run the standard start logic
        // This will still trigger the Fritter safety check you wrote!
        StartDialogue();

        // 4. Optional: If you want to revert back to original data after dialogue ends
        // you can do that in EndDialogue or keep the cutscene data as the new state.
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
        // 1. If the NPC is currently showing the Thank You line, 
        // the very next click MUST close the dialogue and unfreeze the player.
        if (dialogueIndex == dialogueData.thankYouDialogueIndex)
        {
            EndDialogue();
            return;
        }

        // 2. Check the ScriptableObject's "End Line" flags
        if (dialogueIndex < dialogueData.endDialogueLines.Length && dialogueData.endDialogueLines[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        // 3. Normal Advance logic
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
        // 1. Handle Quest Completion/Rewards
        if (questState == QuestState.Completed && dialogueData.quest != null)
        {
            if (!QuestController.Instance.IsQuestHandedIn(dialogueData.quest.QuestID))
            {
                HandleQuestCompletion(dialogueData.quest);
            }
        }

        // 2. Reset Dialogue States
        isDialogueActive = false;
        waitingForGift = false; // ADDED: Safety reset so the NPC isn't stuck waiting next time

        if (DialogueController.Instance != null)
        {
            DialogueController.Instance.EndDialogue();
        }

        // 3. Unfreeze the Game and Player Movement
        PauseController.SetPause(false);

        // Re-enable player movement
        var player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.enabled = true;
        }

        // 4. Kill any active typing coroutines
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

    private void HandleQuestCompletion(Quest quest)
    {
        if (QuestController.Instance.IsQuestActive(quest.QuestID))
        {
            // This tells the GLOBAL controller the quest is done
            QuestController.Instance.HandInQuest(quest.QuestID);

            // Ensure the global controller adds this ID to its handedInQuestIDs list
            if (!QuestController.Instance.handedInQuestIDs.Contains(quest.QuestID))
            {
                QuestController.Instance.handedInQuestIDs.Add(quest.QuestID);
            }

            Debug.Log("Quest Handed In via NPC: " + quest.QuestID);
        }
    }
}