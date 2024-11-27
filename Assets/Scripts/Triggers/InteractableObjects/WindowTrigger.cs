using UnityEngine;

public class WindowTrigger : InteractableObject
{
    public override float interactionRadius { get; } = 1.5f;

    public override string interactionMessage { get; } = "smash the window";

    public GameObject brokenWindowPrefab;
    float interactionTime = 10;

    public override void Interact(Transform interactionOverlayParent)
    {
        var obj = Instantiate(brokenWindowPrefab, Vector3.zero, transform.rotation, interactionOverlayParent);
        obj.transform.position = transform.position;
        DisruptGuard(interactionTime);
    }
}
