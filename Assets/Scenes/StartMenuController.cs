using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    public static bool ShouldLoadGame = false;
    public void OnStartClick()
    {
        ShouldLoadGame = false;
        SceneManager.LoadScene("Cutscene");
    }

    public void OnLoadClick()
    {
        // Simply find the SaveController and tell it to Load.
        // This works because your SaveController is Persistent (DontDestroyOnLoad).
        SaveController saveCtrl = FindObjectOfType<SaveController>();
        if (saveCtrl != null)
        {
            saveCtrl.LoadGame();
        }
        else
        {
            Debug.LogError("SaveController not found in the Menu scene!");
        }
    }
    public void OnExitClick()
    {
#if UNITY_EDITOR 
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}