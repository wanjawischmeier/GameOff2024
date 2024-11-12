using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public Item[] items;

    static ItemDatabase instance;

    private void Awake()
    {
        // initialize singleton
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public static Item GetItem(int itemId)
    {
        if (itemId < 0 || itemId >= instance.items.Length)
        {
            return null;
        }
        else
        {
            return instance.items[itemId];
        }
    }

    public static int GetItemId(Item item)
    {
        for (int itemId = 0; itemId < instance.items.Length; itemId++)
        {
            if (item == instance.items[itemId])
            {
                return itemId;
            }
        }

        return -1;
    }
}
