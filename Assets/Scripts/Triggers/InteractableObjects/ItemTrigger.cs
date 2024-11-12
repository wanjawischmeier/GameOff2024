using System.Collections;
using UnityEngine;

public class ItemTrigger : InteractableObject
{
    public override float interactionRadius { get; } = 1f;

    public override string interactionMessage { get => $"pick up the {item.itemName}"; }

    [HideInInspector]
    public bool isDropped = false;
    public Item item;

    public override void Interact(Transform interactionOverlayParent)
    {
        InventoryManager.Instance.AddItem(item);
        Destroy(gameObject);
    }

    public IEnumerator MarkAsDropped(float destructionDelay = 4)
    {
        isDropped = true;

        yield return new WaitForSeconds(destructionDelay);
        Destroy(gameObject);
    }
}
