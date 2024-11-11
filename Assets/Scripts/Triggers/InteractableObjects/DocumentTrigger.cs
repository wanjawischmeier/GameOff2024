using UnityEngine;

public class DocumentTrigger : InteractableObject
{
    public override float interactionRadius { get; } = 1f;

    public override string interactionMessage { get; } = "pick up the document";

    public Item item;

    public override void Interact(Transform interactionOverlayParent)
    {
        InventoryManager.Instance.AddItem(item);
        RemoveInteractable();
    }
}
