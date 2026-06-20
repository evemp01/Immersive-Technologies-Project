using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class EndGameHUD : MonoBehaviour
{
    private TMP_Text text;

    void Awake() => text = GetComponent<TMP_Text>();

    public void ShowWin()  => SetText("<color=#4EBC36>You WON</color>");
    public void ShowLose() => SetText("<color=#E33333>You LOSE</color>");
    public void Clear()    => SetText("");

    private void SetText(string msg)
    {
        if (text == null) text = GetComponent<TMP_Text>();
        if (text != null) text.text = msg;
    }
}