using UnityEngine;
using System.Collections.Generic;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;
    public List<Achievement> allAchievements;

    [Header("UI Reference")]
    public AchievementPopup popup;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void UnlockAchievement(string name)
    {
        Achievement a = allAchievements.Find(x => x.achievementName == name);
        if (a != null && !a.isUnlocked)
        {
            a.isUnlocked = true;
            Debug.Log("Achievement Unlocked: " + name);
            if (popup != null) popup.Display(a);

            // Here you could also trigger a Steam/PlayStation Trophy API
        }
    }
}