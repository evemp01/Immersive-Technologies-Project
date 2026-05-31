using UnityEngine;

// Stats for one mop. Make new ones via Create -> MopMan -> Mop Type. tileRange is how big an
// area it cleans, neededPasses how many passes a fully dirty tile takes, upgradeIndex its
// place in the upgrade chain.
[CreateAssetMenu(fileName = "Mop", menuName = "MopMan/Mop Type")]
public class MopType : ScriptableObject
{
    public string displayName = "Base Mop";

    [Tooltip("Cleaned area side length. 1 = 1x1, 2 = 2x2, 3 = 3x3.")]
    public int tileRange = 1;

    [Tooltip("Passes needed to fully clean a dirty tile.")]
    public int neededPasses = 2;

    [Tooltip("Order in the upgrade chain: 0 = base, 1 = next, ...")]
    public int upgradeIndex = 0;

    [Header("Appearance")]
    [Tooltip("Head colour, shown on the shelf and on the player's mop when equipped.")]
    public Color headColor = Color.white;

    [Header("Economy")]
    [Tooltip("Unlock cost. 0 for now, until the coin system is in.")]
    public int price = 0;

    public Sprite icon;
}
