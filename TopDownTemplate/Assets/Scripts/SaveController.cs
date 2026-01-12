using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveController : MonoBehaviour
{
    private static SaveController instance;
    private string saveLocation;
    private InventoryController inventoryController;
    private CinemachineConfiner cinemachineConfiner;
    private Transform playerTransform;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindObjectOfType<InventoryController>();
        cinemachineConfiner = FindObjectOfType<CinemachineConfiner>();
    }

    void Start()
    {
        Debug.Log("DEBUG: SaveController Start() called. ShouldLoadGame is: " + StartMenuController.ShouldLoadGame);

        if (StartMenuController.ShouldLoadGame)
        {
            StartCoroutine(LoadDataRoutine());
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This runs automatically every time a new scene finished loading
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("DEBUG: Scene Loaded: " + scene.name + ". ShouldLoadGame is: " + StartMenuController.ShouldLoadGame);

        if (StartMenuController.ShouldLoadGame)
        {
            // Force the load to happen now that the scene is ready
            StartCoroutine(LoadDataRoutine());
        }
    }

    private IEnumerator LoadDataRoutine()
    {
        // Wait for the scene to settle and UI to wake up
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);

        Debug.Log("DEBUG: Applying Save Data now!");

        // Re-find references to ensure we aren't using "Old Scene" variables
        inventoryController = FindObjectOfType<InventoryController>();

        ApplySaveData();

        StartMenuController.ShouldLoadGame = false;
    }

    public void SaveGame()
    {
        inventoryController?.RebuildItemCounts();
        QuestController.Instance?.RefreshQuestProgress();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        // 1. Initialize the data object FIRST
        SaveData saveData = new SaveData();

        // 2. NOW you can assign the music track name
        if (MusicManager.Instance != null)
        {
            saveData.currentMusicTrack = MusicManager.Instance.GetCurrentTrackName();
        }

        saveData.sceneIndex = SceneManager.GetActiveScene().buildIndex;
        saveData.playerPosition = playerObj.transform.position;

        // 3. FIX: Save the Map Boundary name
        cinemachineConfiner = FindObjectOfType<CinemachineConfiner>();
        if (cinemachineConfiner != null && cinemachineConfiner.m_BoundingShape2D != null)
        {
            // This puts "F1" or "T1" into the JSON
            saveData.mapBoundary = cinemachineConfiner.m_BoundingShape2D.gameObject.name;
        }

        // 4. FIX: Save the World Item States (so picked up items stay gone)
        if (WorldStateManager.Instance != null)
        {
            saveData.collectedWorldItemIDs = WorldStateManager.Instance.GetCollectedIDs();
        }

        // 5. Save Quest and Inventory
        saveData.inventorySaveData = inventoryController?.GetInventoryItems() ?? new List<InventorySaveData>();
        saveData.questProgressData = QuestController.Instance?.GetSerializedProgress() ?? new List<QuestProgress>();

        // 6. Write to file
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveLocation, json);
        Debug.Log("Saved Successfully with Music: " + saveData.currentMusicTrack);
    }

    public void LoadGame()
    {
        Debug.Log("DEBUG: Load Button Clicked!"); //

        if (File.Exists(saveLocation))
        {
            // 1. Read the save file
            string json = File.ReadAllText(saveLocation);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("DEBUG: Save file found. Target Scene Index: " + data.sceneIndex);
            // After loading the JSON
            if (!string.IsNullOrEmpty(data.currentMusicTrack))
            {
                MusicManager.Instance.PlayTrackByName(data.currentMusicTrack);
            }

            // 2. Check if we need to change scenes
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            if (currentSceneIndex != data.sceneIndex)
            {
                // Set the flag so the next scene knows to apply data
                StartMenuController.ShouldLoadGame = true;
                Debug.Log("DEBUG: Switching scenes to " + data.sceneIndex);
                SceneManager.LoadScene(data.sceneIndex);
                return; // Exit here; the Start() in the new scene will finish the load
            }

            // 3. If we are already in the right scene, apply data immediately
            ApplySaveData();
        }
        else
        {
            Debug.LogError("DEBUG: No save file found at: " + saveLocation);
        }
    }

    private void ApplySaveData()
    {
        if (!File.Exists(saveLocation)) return;

        string json = File.ReadAllText(saveLocation);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // ADD THIS: Resume the saved music track immediately upon application
        if (MusicManager.Instance != null && !string.IsNullOrEmpty(data.currentMusicTrack))
        {
            MusicManager.Instance.PlayTrackByName(data.currentMusicTrack);
        }

        // 1. Refresh Scene References
        inventoryController = FindObjectOfType<InventoryController>();
        cinemachineConfiner = FindObjectOfType<CinemachineConfiner>();

        // 2. Inventory Logic - DO THIS FIRST
        if (inventoryController != null)
        {
            inventoryController.SetInventoryItems(data.inventorySaveData);
            inventoryController.RebuildItemCounts();
        }
        // 1. Teleporting Player (Keep this first)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = data.playerPosition;
            Debug.Log("DEBUG: Player moved.");

            // MOVE THE CAMERA BLOCK HERE (Right after player move)
            if (!string.IsNullOrEmpty(data.mapBoundary))
            {
                GameObject boundaryObj = GameObject.Find(data.mapBoundary);
                if (boundaryObj != null && cinemachineConfiner != null)
                {
                    // APPLY the saved boundary to the confiner
                    cinemachineConfiner.m_BoundingShape2D = boundaryObj.GetComponent<Collider2D>();

                    // Force the confiner to forget the old shape immediately
                    cinemachineConfiner.InvalidatePathCache();

                    var virtualCamera = cinemachineConfiner.GetComponent<CinemachineVirtualCamera>();
                    if (virtualCamera != null)
                    {
                        // Snap the camera to the player so it doesn't "slide" across the map
                        virtualCamera.OnTargetObjectWarped(player.transform, player.transform.position - data.playerPosition);
                        virtualCamera.ForceCameraPosition(player.transform.position, player.transform.rotation);
                    }
                }
            }

        }
        // 3. Quest & Inventory Logic
        if (QuestController.Instance != null)
        {
            QuestController.Instance.LoadQuestProgress(data.questProgressData, data.handinQuestIDs);
        }

        // 5. Hide Collected World Items - IMPORTANT: Do this BEFORE RefreshUI
        if (WorldStateManager.Instance != null && data.collectedWorldItemIDs != null)
        {
            WorldStateManager.Instance.LoadCollectedStates(data.collectedWorldItemIDs);
        }

        // 6. FINALLY: Refresh the Visuals
        inventoryController?.RefreshUI();

        // 7. NPC Refresh
        NPC[] npcs = FindObjectsOfType<NPC>();
        foreach (NPC npc in npcs) npc.SyncQuestState();

        Debug.Log("--- DEBUG: ApplySaveData COMPLETE ---");
    }

}