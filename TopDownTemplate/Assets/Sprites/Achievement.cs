using UnityEngine;

[CreateAssetMenu(fileName = "New Achievement", menuName = "Systems/Achievement")]
public class Achievement : ScriptableObject
{
    public string achievementName;
    [TextArea] public string description;
    public Sprite icon;
    public bool isUnlocked;
}