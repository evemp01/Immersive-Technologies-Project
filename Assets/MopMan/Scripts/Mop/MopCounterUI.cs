using UnityEngine;
using TMPro;

public class MopCounterUI : MonoBehaviour
{
    private TMP_Text text;

    void Awake() => text = GetComponent<TMP_Text>();

    void Start()
    {
        UpdateMop(null);
    }

    public void UpdateMop(MopType mop)
    {
        if (text == null) text = GetComponent<TMP_Text>();
        if (text == null) return;

        if (mop == null)
        {
            text.text = $"<voffset=0.3em><sprite name=\"no_mop\"></voffset> No mop";
            return;
        }

        string spriteName = GetSpriteName(mop.headColor);
        text.text = $"<voffset=0.3em><sprite name=\"{spriteName}\"></voffset> {mop.displayName}";
    }

    private string GetSpriteName(Color c)
    {
        if (ColorMatch(c, "FFFFFF")) return "mop_white";
        if (ColorMatch(c, "F0F128")) return "mop_yellow";
        if (ColorMatch(c, "00C3E3")) return "mop_blue";
        if (ColorMatch(c, "4EBC36")) return "mop_green";
        return "mop_white";
    }

    private bool ColorMatch(Color c, string hex)
    {
        Color target = HexToColor(hex);
        return Vector3.Distance(
            new Vector3(c.r, c.g, c.b),
            new Vector3(target.r, target.g, target.b)
        ) < 0.05f;
    }

    private Color HexToColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color c);
        return c;
    }
}