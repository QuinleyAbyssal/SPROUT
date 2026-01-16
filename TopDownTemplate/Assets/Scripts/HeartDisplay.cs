using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeartDisplay : MonoBehaviour
{
    public List<Image> heartImages; // Drag your 10 heart images here in the inspector
    public Sprite filledHeart;
    public Sprite emptyHeart;

    public void UpdateHearts(int level)
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            // If the current heart index is less than the level, show it filled
            if (i < level) heartImages[i].sprite = filledHeart;
            else heartImages[i].sprite = emptyHeart;
        }
    }
}