using UnityEngine;

[RequireComponent(typeof(NPC))]
public class CutsceneTrigger : MonoBehaviour
{
    private NPC npcScript;
    public NPCDialogue cutsceneAsset;
    public int startLine = 0;

    private void Awake()
    {
        npcScript = GetComponent<NPC>();
    }

    public void ActivateFritterDialogue()
    {
        if (npcScript != null && cutsceneAsset != null)
        {
            npcScript.StartCutsceneDialogue(cutsceneAsset, startLine);
        }
    }

    // NEW: Call this from Timeline to force-close the dialogue
    public void ForceEndDialogue()
    {
        if (npcScript != null)
        {
            npcScript.EndDialogue(); // This runs your unpause/unfreeze logic
            Debug.Log("Dialogue forced to end by CutsceneTrigger.");
        }
    }
}