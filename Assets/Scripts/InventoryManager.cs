using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public struct InventoryState
    {
        public int selectedSlot;
        public InventorySlot[] slots;
    }

    public struct InventorySlot
    {
        public int count;
        public Item item;
    }

    public GameObject itemTriggerPrefab;
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
                slots = slots.Values.ToArrayPooled()
            };
        }
    }

    Dictionary<int, InventorySlot> slots;
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
    }

    private void Start()
    {
        slots = new Dictionary<int, InventorySlot>(content.childCount);
        slotTransforms = new Transform[content.childCount];

        for (int i = 0; i < content.childCount; i++)
        {
            slots[i] = new InventorySlot()
            {
                count = -1,
                item = null
            };

            slotTransforms[i] = content.GetChild(i);
        }

        SetSelectedSlot(0);
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            for (int i = 1; i <= 9; i++)
            {
                // number keycode
                KeyCode key = KeyCode.Alpha0 + i;

                if (Input.GetKeyDown(key) && i <= slots.Count)
                {
                    SetSelectedSlot(i - 1);
                }
            }
        }

        if (Input.mouseScrollDelta.y < 0)
        {
            SetSelectedSlot((selectedSlot + 1) % slots.Count);
        }
        else if (Input.mouseScrollDelta.y > 0)
        {
            int item = selectedSlot - 1;
            if (item < 0)
            {
                item = slots.Count - 1;
            }

            SetSelectedSlot(item);
        }
    }

    private void SetSelectedSlot(int slotIndex)
    {
        var slotTransform = slotTransforms[selectedSlot];
        slotTransform.localScale = Vector3.one;
        slotTransform.GetComponent<Image>().color = defaultColor;

        selectedSlot = slotIndex;
        slotTransform = slotTransforms[selectedSlot];
        slotTransform.localScale = Vector3.one * selectedScaleFactor;
        slotTransform.GetComponent<Image>().color = selectedColor;

        var slot = slots[selectedSlot];
        if (slot.item == null)
        {
            itemName.gameObject.SetActive(false);
        }
        else
        {
            itemName.gameObject.SetActive(true);
            itemName.text = slot.item.name;
        }
    }

    private Image GetSlotImage(int slotIndex) => slotTransforms[slotIndex].GetChild(0).GetComponent<Image>();

    private Transform GetSlotBubble(int slotIndex) => slotTransforms[selectedSlot].GetChild(1);

    private TextMeshProUGUI GetBubbleText(Transform bubble) => bubble.GetChild(0).GetComponent<TextMeshProUGUI>();

    public void AddItem(Item item)
    {
        int count = 1;

        // if another slot already holds the item, switch to that
        for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
        {
            if (slots[slotIndex].item == item)
            {
                SetSelectedSlot(slotIndex);
            }
        }

        var oldItem = slots[selectedSlot].item;
        if (oldItem == null)
        {
            itemName.gameObject.SetActive(true);
        }
        else
        {
            // there's already an item in the slot
            var bubble = GetSlotBubble(selectedSlot);

            if (oldItem == item)
            {
                count += slots[selectedSlot].count;
                bubble.gameObject.SetActive(true);
                GetBubbleText(bubble).text = count.ToString();
            }
            else
            {
                bubble.gameObject.SetActive(false);
                DropItems(selectedSlot, true);
            }
        }

        var slot = new InventorySlot()
        {
            count = count,
            item = item
        };
        slots[selectedSlot] = slot;

        var slotImage = GetSlotImage(selectedSlot);
        slotImage.sprite = item.sprite;
        slotImage.enabled = true;

        itemName.text = item.name;
    }

    public bool RemoveItems(int slotIndex, bool removeAll = false)
    {
        if (!slots.ContainsKey(slotIndex))
        {
            // no item in the slot
            return false;
        }

        var bubble = GetSlotBubble(selectedSlot);
        var slot = slots[slotIndex];
        if (slot.count == 1 || removeAll)
        {
            slot.item = null;

            var slotImage = GetSlotImage(slotIndex);
            slotImage.sprite = null;
            slotImage.enabled = false;

            itemName.gameObject.SetActive(false);
            bubble.gameObject.SetActive(false);
        }
        else
        {
            slot.count--;
            GetBubbleText(bubble).text = slot.count.ToString();
        }

        slots[slotIndex] = slot;
        return true;
    }

    private void DropItem(InventorySlot slot)
    {
        var itemPosition = PlayerController.Transform.position.RandomlySpreadVector(itemDropPositionSpread);
        var itemRotation = PlayerController.Instance.bodyTransform.eulerAngles.RandomlySpreadVector(itemDropRotationSpread);
        itemRotation.z += 90;    // rotate item by 90 degrees

        var itemObj = Instantiate(itemTriggerPrefab,
            itemPosition, Quaternion.Euler(itemRotation),
            StaticObjects.InteractionTriggerParent
        );
        itemObj.name = $"{slot.item.name} Trigger";

        var itemTrigger = itemObj.GetComponent<ItemTrigger>();
        itemTrigger.item = slot.item;

        var itemRenderer = itemObj.GetComponent<SpriteRenderer>();
        itemRenderer.sprite = slot.item.sprite;

        InteractionManager.Instance.AddInteractable(itemTrigger);
    }

    public bool DropItems() => DropItems(selectedSlot);

    public bool DropItems(int slotIndex, bool dropAll = false)
    {
        if (slots[slotIndex].item == null)
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
                DropItem(slot);
            }
        }
        else
        {
            DropItem(slot);
        }

        PlayerController.Instance.Interact();
        return RemoveItems(slotIndex);
    }
}
