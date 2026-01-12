using UnityEngine;

public class DialogueSignalBridge : MonoBehaviour
{
    // This method is what you will call from the Signal Receiver
    public void CallDialogue(NPCDialogue data) => DialogueController.Instance.StartDialogue(data, 0);
}