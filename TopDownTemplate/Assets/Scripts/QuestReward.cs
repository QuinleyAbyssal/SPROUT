using System;
using UnityEngine;

// --- REWARD DEFINITION (WHAT THE PLAYER GETS) ---
[Serializable]
public class QuestReward
{
    public RewardType type;

    // Use ItemData for Item rewards so you can drag-and-drop the asset
    public ItemData itemReward;

    // For Gold, Experience, or Item counts
    public int amount = 1;

    // Optional: Reference to a custom script or event for unique rewards
    public string customRewardID;
}

public enum RewardType { Item, Gold, Experience, Custom }