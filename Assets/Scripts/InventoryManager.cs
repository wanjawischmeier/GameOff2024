using System.Collections.Generic;
using TMPro;
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
        public string name;
    }

    public Transform content;
    public TextMeshProUGUI itemName;
    public Color defaultColor, selectedColor;
    public float selectedScaleFactor = 1.1f;

    public static InventoryManager Instance { get; private set; }

    public InventoryState inventoryState
    {
        get
        {
            var slotStates = new InventorySlot[slotTransforms.Length];
            for (int i = 0; i < slotTransforms.Length; i++)
            {
                if (slots.ContainsKey(i))
                {
                    slotStates[i] = slots[i];
                }
            }

            return new InventoryState()
            {
                selectedSlot = selectedSlot,
                slots = slotStates
            };
        }
    }

    Transform[] slotTransforms;
    Dictionary<int, InventorySlot> slots;
    int selectedSlot = 0;
    int items = 0;

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
        slots = new Dictionary<int, InventorySlot>();
        slotTransforms = new Transform[content.childCount];
        for (int i = 0; i < slotTransforms.Length; i++)
        {
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

                if (Input.GetKeyDown(key) && i <= slotTransforms.Length)
                {
                    SetSelectedSlot(i - 1);
                }
            }
        }

        if (Input.mouseScrollDelta.y < 0)
        {
            SetSelectedSlot((selectedSlot + 1) % slotTransforms.Length);
        }
        else if (Input.mouseScrollDelta.y > 0)
        {
            int item = selectedSlot - 1;
            if (item < 0)
            {
                item = slotTransforms.Length - 1;
            }

            SetSelectedSlot(item);
        }
    }

    private void SetSelectedSlot(int slotIndex)
    {
        var slot = slotTransforms[selectedSlot];
        slot.localScale = Vector3.one;
        slot.GetComponent<Image>().color = defaultColor;

        selectedSlot = slotIndex;
        slot = slotTransforms[selectedSlot];
        slot.localScale = Vector3.one * selectedScaleFactor;
        slot.GetComponent<Image>().color = selectedColor;

        if (slot.childCount == 0)
        {
            itemName.gameObject.SetActive(false);
        }
        else
        {
            itemName.gameObject.SetActive(true);
            itemName.text = slot.GetChild(0).name;
        }
    }

    public bool AddItem(GameObject preFab, string name)
    {
        if (items == slotTransforms.Length - 1)
        {
            // hotbar already full
            return false;
        }

        var slot = new InventorySlot()
        {
            count = 1,
            name = name
        };
        slots.Add(items, slot);

        var item = Instantiate(preFab, slotTransforms[items]);
        item.name = name;
        items++;

        if (selectedSlot == items - 1)
        {
            // show the name if this item is selected
            itemName.gameObject.SetActive(true);
            itemName.text = name;
        }

        return true;
    }
}
