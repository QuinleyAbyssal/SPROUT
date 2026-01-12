using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    private void Awake()
    {
        // Ensure the UI and Controller are at the top level for persistence
        transform.SetParent(null);

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Decide whether to Load an old save or start a New Game
        if (StartMenuController.ShouldLoadGame)
        {
            LoadGame();
            StartMenuController.ShouldLoadGame = false;
        }
        else
        {
            InitializeNewGame();
        }
    }

    private void InitializeNewGame()
    {
        // Clears the UI and internal data to prevent items carrying over
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.ClearInventory();
        }

        // Reset dialogue progress if applicable
        if (DialogueController.Instance != null)
        {
            DialogueController.Instance.completedDialogueIDs.Clear();
        }
    }

    // --- Public Centralized Controls ---

    public void SaveGame()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame();
        }
    }

    public void LoadGame()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.LoadGame();
        }
    }

    public void QuitToMainMenu()
    {
        // 1. Save current progress
        SaveGame();

        // 2. Find the Pause/Exit Menu UI object and turn it off
        // Replace "PauseMenu" with the exact name of your UI panel object
        GameObject pauseMenu = GameObject.Find("Menu");
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        // 3. Load the Menu scene (Index 0)
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}