using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public Transform content;
    public TextMeshProUGUI itemName;
    public Color defaultColor, selectedColor;
    public float selectedScaleFactor = 1.1f;

    Transform[] slots;
    int selectedSlot = 0;
    int items = 0;

    private void Start()
    {
        slots = new Transform[content.childCount];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = content.GetChild(i);
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
            int item = selectedSlot - 1;
            if (item < 0)
            {
                item = slots.Length - 1;
            }

            SetSelectedSlot(item);
        }
    }

    private void SetSelectedSlot(int slotIndex)
    {
        var slot = slots[selectedSlot];
        slot.localScale = Vector3.one;
        slot.GetComponent<Image>().color = defaultColor;

        selectedSlot = slotIndex;
        slot = slots[selectedSlot];
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
        if (items == slots.Length - 1)
        {
            // hotbar already full
            return false;
        }

        var item = Instantiate(preFab, slots[items]);
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
