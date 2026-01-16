using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class AchievementPopup : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text nameText;
    public Image iconImage;

    public void Display(Achievement ach)
    {
        nameText.text = ach.achievementName;
        iconImage.sprite = ach.icon;
        StartCoroutine(ShowAndHide());
    }

    IEnumerator ShowAndHide()
    {
        panel.SetActive(true);
        // Add a simple animation or just wait
        yield return new WaitForSeconds(5f);
        panel.SetActive(false);
    }
}