using System.Collections;
using UnityEngine;

public class ItemTrigger : InteractableObject
{
    public override float interactionRadius { get; } = 1;

    public override string interactionMessage { get => $"pick up the {item.itemName}"; }

    [HideInInspector]
    public bool isDropped = false;
    public Item item;

    public override void Interact(Transform interactionOverlayParent)
    {
        if (item.showInInventory)
        {
            InventoryManager.Instance.AddItem(item);
        }
        else
        {
            Debug.Log($"Found story relevant {item.itemName}, adding to game save");
            StoryStateManager.newCollectedItems.Add(item.itemId);
            StoryStateManager.SaveStoryState();
        }

        Destroy(gameObject);
    }

    public IEnumerator MarkAsDropped(float destructionDelay = 4)
    {
        isDropped = true;

        yield return new WaitForSeconds(destructionDelay);
        Destroy(gameObject);
    }
}
