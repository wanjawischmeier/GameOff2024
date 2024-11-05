public class WindowTrigger : InteractableObject
{
    public override float interactionRadius { get; } = 1.5f;

    public override string interactionMessage { get; } = "smash the window";

    float interactionTime = 10;

    public override void Interact()
    {
        DisruptGuard(interactionTime);
    }
}
