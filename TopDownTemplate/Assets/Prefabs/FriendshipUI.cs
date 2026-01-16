using UnityEngine;
using UnityEngine.UI;

public class FriendshipUI : MonoBehaviour
{
    public Image[] heartIcons;        // Drag your 5 heart images here in Inspector
    public Sprite fullHeart;          // Drag your red heart sprite here
    public Sprite emptyHeart;         // Drag your gray heart sprite here
    public GameObject heartContainer; // Drag the parent object of the hearts here
    public static FriendshipUI Instance; // Add this line
    // Call this to force the hearts away (useful for Fritter cutscenes)
    private void Awake()
    {
        Instance = this; // Set the instance
    }
    public void HideHearts()
    {
        if (heartContainer != null)
        {
            heartContainer.SetActive(false); // Disable the entire heart row
        }
    }

    public void UpdateHeartDisplay(string npcName)
    {
        if (heartContainer == null) return;

        // 1. Check for special cases (Fritter/Player)
        if (string.IsNullOrEmpty(npcName) || npcName == "Player" || npcName == "Fritter")
        {
            heartContainer.SetActive(false);
            return;
        }

        // 2. Show container for regular NPCs
        heartContainer.SetActive(true);

        // 3. Update the heart sprites based on level
        if (FriendshipManager.Instance != null)
        {
            int currentLevel = FriendshipManager.Instance.GetLevel(npcName);
            for (int i = 0; i < heartIcons.Length; i++)
            {
                heartIcons[i].sprite = (i < currentLevel) ? fullHeart : emptyHeart;
                // Set full hearts to bright white, empty hearts to semi-transparent
                heartIcons[i].color = (i < currentLevel) ? Color.white : new Color(1, 1, 1, 0.5f);
            }
        }
    } // <--- Make sure this brace exists!
}