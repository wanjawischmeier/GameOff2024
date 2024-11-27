using UnityEngine;

public class SinkTrigger : InteractableObject
{
    public override float interactionRadius { get; } = 1.5f;

    public override string interactionMessage { get; } = "overflow the sink";

    public GameObject overflowingSinkPrefab;
    float interactionTime = 4;

    public override void Interact(Transform interactionOverlayParent)
    {
        Instantiate(overflowingSinkPrefab, transform.position, transform.rotation, interactionOverlayParent);
        DisruptGuard(interactionTime);
    }
}
