using TMPro;
using UnityEngine;

public class QuestEntryUI : MonoBehaviour
{
    public TextMeshProUGUI objectiveText;

    // This method now only accepts ONE string (the objective)
    public void Setup(string objective)
    {
        if (objectiveText != null)
            objectiveText.text = objective;
    }
}