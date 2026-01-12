// QuestDictionary.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// This should typically be a ScriptableObject that holds all your Quest assets,
// allowing you to easily look them up by ID.
public class QuestDictionary : MonoBehaviour
{
    public static QuestDictionary Instance { get; private set; }

    // This should hold references to all your Quest ScriptableObjects
    [SerializeField] private Quest[] allQuestsArray;
    private Dictionary<string, Quest> questMap;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        BuildQuestMap();
    }

    private void BuildQuestMap()
    {
        // Initializes the dictionary for quick look-up
        questMap = allQuestsArray.ToDictionary(q => q.QuestID, q => q);
    }

    // This is the required method for QuestController.LoadQuestProgress
    public Quest GetQuestByID(string id)
    {
        // If the map is null or empty, force a rebuild immediately
        if (questMap == null || questMap.Count == 0) BuildQuestMap();

        if (questMap.TryGetValue(id, out Quest quest))
        {
            return quest;
        }

        Debug.LogError($"Quest ID {id} not found in Dictionary! Check the Inspector array.");
        return null;
    }
}