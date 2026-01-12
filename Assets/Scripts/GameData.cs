// Define this class outside the SaveLoadManager or in its own file
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // --- SAVED QUEST DATA ---

    // This list holds all active (QuestProgress) objects.
    // CRITICAL FIX: Always initialize lists here as well.
    public List<QuestProgress> questProgressData = new List<QuestProgress>();

    // --- EXAMPLE OF OTHER SAVED DATA ---

    // CRITICAL FIX: All lists that are saved must be initialized like this.
    public List<int> collectedItemIDs = new List<int>();

    public int currentSceneIndex;
    public float playerHealth;

    // Parameterless constructor for deserialization
    public GameData()
    {
        // All fields are initialized by their declarations above.
    }
}