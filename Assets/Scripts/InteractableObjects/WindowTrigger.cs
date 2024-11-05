using UnityEngine;

public class WindowTrigger : InteractableObject
{
    public override float interactionRadius { get; } = 1.5f;

    public override string interactionMessage { get; } = "smash the window";

    public GameObject brokenWindowPrefab;
    float interactionTime = 10;

    public override void Interact(Transform interactionOverlayParent)
    {
        DisruptGuard(interactionTime, brokenWindowPrefab, interactionOverlayParent);
    }
}
