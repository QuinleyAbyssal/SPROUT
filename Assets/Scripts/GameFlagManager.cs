using UnityEngine;

public static class GameFlagManager
{
    // A flag to track if the required dialogue has finished.
    // Make this static so it's globally accessible.
    public static bool HasDialogueCompleted { get; private set; } = false;

    // Call this method when the required dialogue sequence ends.
    public static void SetDialogueComplete()
    {
        HasDialogueCompleted = true;
        Debug.Log("Game Flag: Dialogue Completion set to TRUE. Items are now interactable.");
    }

    // Call this to reset the flag if needed (e.g., for starting a new game).
    public static void ResetFlags()
    {
        HasDialogueCompleted = false;
    }
}