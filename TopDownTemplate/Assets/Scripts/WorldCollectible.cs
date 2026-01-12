// WorldCollectible.cs
using UnityEngine;
using System;

/// <summary>
/// Attached to any persistent collectible item in the world. 
/// Gives the item a unique Instance ID for saving world state.
/// </summary>
public class WorldCollectible : MonoBehaviour
{
    // --- DESIGNER INPUT ---
    // The unique ID of this instance. MUST be unique across the entire game world.
    [Header("Unique Instance ID")]
    [Tooltip("This ID is saved when the item is collected. Must be unique.")]
    [SerializeField]
    private string uniqueID;

    // Public accessor for other scripts to read the ID
    public string UniqueID => uniqueID;

    // The ItemTypeID is stored here too for convenient pickup logic
    [Header("Item Type ID (Shared)")]
    [Tooltip("The ID used by ItemDictionary and InventoryController.")]
    public int ItemTypeID;

    private void OnValidate()
    {
        // This helper ensures designers automatically assign a unique ID in the editor
        if (string.IsNullOrEmpty(uniqueID))
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                // Generates a GUID to ensure uniqueness
                uniqueID = "COLLECTIBLE_" + Guid.NewGuid().ToString().Substring(0, 8);
            }
        }
    }
}
