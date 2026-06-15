using UnityEngine;
using UnityEngine.Rendering;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class HudAlwaysOnTop : MonoBehaviour
{
    void LateUpdate()
    {
        var renderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (var r in renderers)
            foreach (var m in r.materials)
                m.SetInt("unity_GUIZTestMode", (int)CompareFunction.Always);
    }
}