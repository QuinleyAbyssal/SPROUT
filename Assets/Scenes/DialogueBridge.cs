using UnityEngine;

public class DialogueBridge : MonoBehaviour
{
    public void CallDialogue(NPCDialogue data) => DialogueController.Instance.StartDialogue(data, 0);
}