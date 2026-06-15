using UnityEngine;
using TMPro;

public class LivesCounterUI : MonoBehaviour
{
    [SerializeField] private string heartSpriteName = "heart";
    private TMP_Text text;

    void Awake() => text = GetComponent<TMP_Text>();

    public void SetLives(int current, int max)
    {
        if (text == null) text = GetComponent<TMP_Text>();
        if (text == null)
        {
            Debug.LogWarning("LivesCounterUI: manca il TextMeshPro su " + name);
            return;
        }
        text.text = $"<voffset=0.3em><sprite name=\"{heartSpriteName}\"></voffset> {current}/{max}";
    }
}