using UnityEngine;

// This part defines the Data Asset
[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string dialogueID;
    public string npcName;
    public Sprite npcPortrait;
    [TextArea(3, 10)] public string[] dialogueLines;
    public bool[] autoProgressLines;
    public bool[] endDialogueLines;
    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.05f;
    public AudioClip voiceSound;
    public float voicePitch = 1f;

    public DialogueChoice[] choices; // This uses the class defined below

    [Header("Quest Indexes")]
    public Quest quest;
    public int DefaultStartIndex;
    public int questInProgressIndex;
    public int questCompletedIndex;
    public int postQuestIndex;
}

// THIS PART MUST BE OUTSIDE THE PREVIOUS CURLY BRACES
[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex; // When do these choices appear?
    public string[] choices; // What text is on the buttons?
    public int[] nextDialogueIndexes; // Where does each button lead?
    public bool[] givesQuest; // Does this button grant a quest?
}