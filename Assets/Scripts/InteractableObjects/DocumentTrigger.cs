using UnityEngine;

public class DocumentTrigger : InteractableObject
{
    public override float interactionRadius { get; } = 1f;

    public override string interactionMessage { get; } = "pick up the document";

    public GameObject preFab;
    public InventoryManager inventoryManager;
    public string documentName;

    public override void Interact(Transform interactionOverlayParent)
    {
        if (inventoryManager.AddItem(preFab, documentName))
        {
            RemoveInteractable();
        }
    }
}
