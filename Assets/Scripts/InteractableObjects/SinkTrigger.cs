public class SinkTrigger : InteractableObject
{
    public override float interactionRadius { get; } = 1.5f;

    public override string interactionMessage { get; } = "overflow the sink";
    
    float interactionTime = 4;

    public override void Interact()
    {
        DisruptGuard(interactionTime);
    }
}
