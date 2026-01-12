using UnityEngine;
using UnityEngine.SceneManagement;

public class TimelineSceneLoader : MonoBehaviour
{
    // Make this function PUBLIC so the Timeline Signal can call it
    public void LoadNextSceneViaTimeline()
    {
        Debug.Log("Timeline finished. Loading next scene.");
        // Use the same loading logic
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}