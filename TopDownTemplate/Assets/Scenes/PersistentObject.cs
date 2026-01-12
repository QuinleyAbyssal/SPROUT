using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private void Awake()
    {
        // This ensures the UI object and all its children (DialoguePanel, etc.) carry over
        DontDestroyOnLoad(gameObject);
    }
}