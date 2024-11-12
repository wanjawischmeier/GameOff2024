using UnityEngine;

[CreateAssetMenu]
public class Item : ScriptableObject
{
    public Sprite sprite;
    public string itemName;

    public int itemId
    {
        get => ItemDatabase.GetItemId(this);
    }
}