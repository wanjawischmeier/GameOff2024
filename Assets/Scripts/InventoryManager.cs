using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [System.Serializable]
    public struct InventoryState
    {
        public int selectedSlot;
        public InventorySlot[] slots;
    }

    [System.Serializable]
    public struct InventorySlot
    {
        public int count, itemId;

        public Item item
        {
            get => ItemDatabase.GetItem(itemId);
        }
    }

    public GameObject itemDroppedPrefab;
    public Transform content;
    public TextMeshProUGUI itemName;
    public Color defaultColor, selectedColor;
    public float selectedScaleFactor = 1.1f;

    public static InventoryManager Instance { get; private set; }

    public Item selectedItem { get => slots[selectedSlot].item; }

    public InventoryState inventoryState
    {
        get
        {
            return new InventoryState()
            {
                selectedSlot = selectedSlot,
                slots = slots
            };
        }

        set
        {
            InventorySlot slot;

            for (int slotIndex = 0; slotIndex < value.slots.Length; slotIndex++)
            {
                slot = value.slots[slotIndex];
                selectedSlot = slotIndex;
                AddItem(slot.itemId, slot.count);
            }

            selectedSlot = value.selectedSlot;
        }
    }

    string saveFilePath
    {
        get => Path.Combine(Application.persistentDataPath + "inventory.save");
    }

    InventorySlot[] slots;
    Transform[] slotTransforms;
    int selectedSlot = 0;

    const float itemDropPositionSpread = 0.1f;
    const float itemDropRotationSpread = 10;

    private void Awake()
    {
        // initialize singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        slots = new InventorySlot[content.childCount];
        slotTransforms = new Transform[content.childCount];

        for (int i = 0; i < content.childCount; i++)
        {
            slots[i] = new InventorySlot()
            {
                count = -1,
                itemId = -1
            };

            slotTransforms[i] = content.GetChild(i);
        }
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            for (int i = 1; i <= 9; i++)
            {
                // number keycode
                KeyCode key = KeyCode.Alpha0 + i;

                if (Input.GetKeyDown(key) && i <= slots.Length)
                {
                    SetSelectedSlot(i - 1);
                }
            }
        }

        if (Input.mouseScrollDelta.y < 0)
        {
            SetSelectedSlot((selectedSlot + 1) % slots.Length);
        }
        else if (Input.mouseScrollDelta.y > 0)
        {
            int slotIndex = selectedSlot - 1;
            if (slotIndex < 0)
            {
                slotIndex = slots.Length - 1;
            }

            SetSelectedSlot(slotIndex);
        }
    }

    private Image GetSlotImage(int slotIndex) => slotTransforms[slotIndex].GetChild(0).GetComponent<Image>();

    private Transform GetSlotBubble(int slotIndex) => slotTransforms[selectedSlot].GetChild(1);

    private TextMeshProUGUI GetBubbleText(Transform bubble) => bubble.GetChild(0).GetComponent<TextMeshProUGUI>();

    /// <summary>
    /// The scene state manager is responsible for calling this function once the inventory state is loaded
    /// </summary>
    /// <param name="slotIndex"></param>
    public void SetSelectedSlot(int slotIndex)
    {
        var slotTransform = slotTransforms[selectedSlot];
        slotTransform.localScale = Vector3.one;
        slotTransform.GetComponent<Image>().color = defaultColor;

        selectedSlot = slotIndex;
        slotTransform = slotTransforms[selectedSlot];
        slotTransform.localScale = Vector3.one * selectedScaleFactor;
        slotTransform.GetComponent<Image>().color = selectedColor;

        var slot = slots[selectedSlot];
        if (slot.itemId == -1)
        {
            itemName.gameObject.SetActive(false);
        }
        else
        {
            itemName.gameObject.SetActive(true);
            itemName.text = slot.item.name;
        }
    }

    public void AddItem(Item item) => AddItem(item.itemId);

    public void AddItem(int itemId, int count = 1)
    {
        if (itemId == -1) return;
        int totalCount = count;

        // if another slot already holds the item, switch to that
        for (int slotIndex = 0; slotIndex < slots.Length; slotIndex++)
        {
            if (slots[slotIndex].itemId == itemId)
            {
                SetSelectedSlot(slotIndex);
            }
        }

        int oldItemId = slots[selectedSlot].itemId;
        if (oldItemId == -1)
        {
            itemName.gameObject.SetActive(true);
        }
        else
        {
            // there's already an item in the slot
            if (oldItemId == itemId)
            {
                totalCount += slots[selectedSlot].count;
            }
            else
            {
                DropItem(selectedSlot, true);
            }
        }

        var bubble = GetSlotBubble(selectedSlot);
        if (totalCount > 1)
        {
            bubble.gameObject.SetActive(true);
            GetBubbleText(bubble).text = totalCount.ToString();
        }
        else
        {
            bubble.gameObject.SetActive(false);
        }

        var slot = new InventorySlot()
        {
            count = totalCount,
            itemId = itemId
        };
        slots[selectedSlot] = slot;

        var slotImage = GetSlotImage(selectedSlot);
        var item = ItemDatabase.GetItem(itemId);
        slotImage.sprite = item.sprite;
        slotImage.enabled = true;

        itemName.text = item.name;
    }

    public bool RemoveItems(int slotIndex, bool removeAll = false)
    {
        if (slots[slotIndex].itemId == -1)
        {
            // no item in the slot
            return false;
        }

        var bubble = GetSlotBubble(selectedSlot);
        var slot = slots[slotIndex];
        if (slot.count == 1 || removeAll)
        {
            slot.itemId = -1;

            var slotImage = GetSlotImage(slotIndex);
            slotImage.sprite = null;
            slotImage.enabled = false;

            itemName.gameObject.SetActive(false);
            bubble.gameObject.SetActive(false);
        }
        else
        {
            slot.count--;
        }

        if (slot.count == 1)
        {
            bubble.gameObject.SetActive(false);
        }
        else
        {
            GetBubbleText(bubble).text = slot.count.ToString();
        }

        slots[slotIndex] = slot;
        SceneStateManager.SaveInventoryState();
        return true;
    }

    private void InstantiateDroppedItem(InventorySlot slot)
    {
        var itemPosition = PlayerController.Transform.position.RandomlySpreadVector(itemDropPositionSpread);
        var itemRotation = PlayerController.Instance.bodyTransform.eulerAngles.RandomlySpreadVector(itemDropRotationSpread);
        itemRotation.z += 90;    // rotate item by 90 degrees

        var itemObj = Instantiate(itemDroppedPrefab,
            itemPosition, Quaternion.Euler(itemRotation),
            StaticObjects.InteractionTriggerParent
        );
        itemObj.name = $"{slot.item.name} Trigger";

        var itemTrigger = itemObj.GetComponent<ItemTrigger>();
        itemTrigger.item = slot.item;
        StartCoroutine(itemTrigger.MarkAsDropped());

        var itemRenderer = itemObj.GetComponent<SpriteRenderer>();
        itemRenderer.sprite = slot.item.sprite;
    }

    public void DropItem() => DropItem(selectedSlot);

    public bool DropItem(int slotIndex, bool dropAll = false)
    {
        if (slots[slotIndex].itemId == -1)
        {
            // no item in the slot
            return false;
        }

        // spread items around the player
        var slot = slots[slotIndex];
        if (dropAll)
        {
            for (int i = 0; i < slot.count; i++)
            {
                InstantiateDroppedItem(slot);
            }
        }
        else
        {
            InstantiateDroppedItem(slot);
        }

        PlayerController.Instance.Interact();
        return RemoveItems(slotIndex);
    }
}
