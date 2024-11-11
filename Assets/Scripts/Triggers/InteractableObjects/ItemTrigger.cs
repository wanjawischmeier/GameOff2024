using UnityEngine;

public class ItemTrigger : InteractableObject
{
    public override float interactionRadius { get; } = 1f;

    public override string interactionMessage { get => $"pick up the {item.itemName}"; }

    public Item item;

    public override void Interact(Transform interactionOverlayParent)
    {
        InventoryManager.Instance.AddItem(item);
        RemoveInteractable();
    }
}
