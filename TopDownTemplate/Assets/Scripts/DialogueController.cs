using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;
    [Header("Save Data")]
    public List<string> completedDialogueIDs = new List<string>();

    [field: Header("Settings")]
    public bool IsDialogueActive { get; private set; }
    public bool IsTyping { get; private set; }

    private NPCDialogue currentDialogue;
    private int currentLineIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartDialogue(NPCDialogue dialogueData, int startIndex)
    {
        // 1. Reconnect to the CURRENT scene's UI based on your hierarchy
        ReconnectUI();

        currentDialogue = dialogueData;
        currentLineIndex = startIndex;
        IsDialogueActive = true;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        // 2. Push the Asset data (Fritter, Sprite) to the UI components
        UpdateUIElements();

        DisplayLine();
    }
    public void UpdateLine(int newIndex)
    {
        currentLineIndex = newIndex;
        DisplayLine();
    }

    private void UpdateUIElements()
    {
        if (currentDialogue == null) return;

        // Apply NPC Name from Asset
        if (nameText != null) nameText.text = currentDialogue.npcName;

        // Apply Portrait from Asset
        if (portraitImage != null)
        {
            if (currentDialogue.npcPortrait != null)
            {
                portraitImage.sprite = currentDialogue.npcPortrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
            }
        }
    }

    public void ReconnectUI()
    {
        // Finds the "UI" object from your Hierarchy image
        GameObject uiRoot = GameObject.Find("UI");

        if (uiRoot != null)
        {
            // Search children for DialoguePanel (even if inactive)
            Transform panelTrans = uiRoot.transform.Find("DialoguePanel");
            if (panelTrans != null)
            {
                dialoguePanel = panelTrans.gameObject;

                // MAPPING TO YOUR EXACT NAMES (from image_6e41b3.png)
                dialogueText = panelTrans.Find("DialogueText")?.GetComponent<TMP_Text>();
                nameText = panelTrans.Find("NPCNameText")?.GetComponent<TMP_Text>();

                Transform portraitTrans = panelTrans.Find("DialoguePortrait");
                if (portraitTrans != null) portraitImage = portraitTrans.GetComponent<Image>();

                choiceContainer = panelTrans.Find("ChoicesPanel");
            }
        }
    }

    private void DisplayLine()
    {
        StopAllCoroutines();
        if (gameObject.activeInHierarchy) StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        IsTyping = true;
        dialogueText.text = "";
        string line = currentDialogue.dialogueLines[currentLineIndex];

        foreach (char letter in line)
        {
            dialogueText.text += letter;
            if (currentDialogue.voiceSound != null)
            {
                SoundEffectManager.PlayVoice(currentDialogue.voiceSound, currentDialogue.voicePitch);
            }
            yield return new WaitForSeconds(currentDialogue.typingSpeed);
        }

        IsTyping = false;

        // Auto-progress logic for your Cutscene
        if (currentDialogue.autoProgressLines.Length > currentLineIndex && currentDialogue.autoProgressLines[currentLineIndex])
        {
            yield return new WaitForSeconds(currentDialogue.autoProgressDelay);

            if (currentDialogue.endDialogueLines.Length > currentLineIndex && currentDialogue.endDialogueLines[currentLineIndex])
                EndDialogue();
            else
                DisplayNextLine();
        }
    }

    public void DisplayNextLine()
    {
        currentLineIndex++;
        if (currentDialogue != null && currentLineIndex < currentDialogue.dialogueLines.Length)
            DisplayLine();
        else
            EndDialogue();
    }

    public void EndDialogue()
    {
        IsDialogueActive = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }


public void ClearChoices() // Fixes the RectTransform crash
    {
        if (choiceContainer == null) return;
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);
    }

    // --- Save/Load Bridge ---
    public void SetCompletedDialogues(List<string> savedIDs)
    {
        completedDialogueIDs = savedIDs ?? new List<string>();
    }


    public bool ChoiceActive()
    {
        return choiceContainer.childCount > 0;
    }

    // --- Internal Typewriter Logic ---
    void Update()
    {
        if (IsDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (IsTyping)
            {
                SkipTyping();
            }
            else if (choiceContainer.childCount == 0) // Only progress if not waiting for a choice
            {
                DisplayNextLine();
            }
        }
    }

    public void SkipTyping()
    {
        if (IsTyping)
        {
            StopAllCoroutines(); // Stops the TypeLine coroutine immediately
            dialogueText.text = currentDialogue.dialogueLines[currentLineIndex]; // Shows full line
            IsTyping = false;

            // If it's an auto-progress line, we need to restart the timer manually here
            if (currentDialogue.autoProgressLines.Length > currentLineIndex && currentDialogue.autoProgressLines[currentLineIndex])
            {
                StartCoroutine(WaitToAutoProgress());
            }
        }
    }

    // Helper to handle the delay if a player skips the typing
    IEnumerator WaitToAutoProgress()
    {
        yield return new WaitForSeconds(currentDialogue.autoProgressDelay);
        DisplayNextLine();
    }

    public void CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction onClickAction)
    {
        if (choiceButtonPrefab == null || choiceContainer == null)
        {
            Debug.LogError("DialogueController: Prefab or Container missing!");
            return;
        }

        GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceContainer);

        // Set the button text
        TMP_Text btnText = buttonObj.GetComponentInChildren<TMP_Text>();
        if (btnText != null) btnText.text = choiceText;

        // Set the button action
        Button btn = buttonObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(onClickAction);
        }
    }
}