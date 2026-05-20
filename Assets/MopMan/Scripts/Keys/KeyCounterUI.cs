using UnityEngine;
using TMPro; 

[RequireComponent(typeof(TMP_Text))] 
public class KeyCounterUI : MonoBehaviour
{
    private TMP_Text uiText; 

    void Awake()
    {
        // Automatically grab the TextMeshPro component 
        uiText = GetComponent<TMP_Text>();
    }

    public void UpdateText(string newText) => uiText.text = newText;
}