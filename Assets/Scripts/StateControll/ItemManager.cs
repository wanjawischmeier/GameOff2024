using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public ItemDatabase itemDatabase;

    static ItemManager instance;

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
        if (itemId < 0 || itemId >= instance.itemDatabase.items.Length)
        {
            return null;
        }
        else
        {
            return instance.itemDatabase.items[itemId];
        }
    }

    public static int GetItemId(Item item)
    {
        for (int itemId = 0; itemId < instance.itemDatabase.items.Length; itemId++)
        {
            if (item == instance.itemDatabase.items[itemId])
            {
                return itemId;
            }
        }

        return -1;
    }
}
