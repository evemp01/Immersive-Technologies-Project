using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class CoinCounterUI : MonoBehaviour
{
    private TMP_Text uiText;

    void Awake() => uiText = GetComponent<TMP_Text>();

    public void UpdateText(string newText) => uiText.text = newText;
}