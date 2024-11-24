using UnityEngine;

[CreateAssetMenu]
public class Item : ScriptableObject
{
    public Sprite sprite;
    public string itemName;
    /// <summary>
    /// item will not be kept in inventory after night (if to be used for story)
    /// </summary>
    public bool storyItem;
    /// <summary>
    /// otherwise the item will be added to the game state collected items immediately
    /// </summary>
    public bool showInInventory = true;
    public bool throwableDistraction = false;

    public int itemId
    {
        get => ItemManager.GetItemId(this);
    }
}