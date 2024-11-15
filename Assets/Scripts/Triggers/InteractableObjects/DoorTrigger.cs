using UnityEngine;

public class DoorTrigger : InteractableObject
{
    public override float interactionRadius { get; } = 1.5f;

    public override string interactionMessage { get => $"open the door to the {houseName}"; }

    public string houseName = "house";
    public int targetSceneBuildIndex;

    public override void Interact(Transform interactionOverlayParent)
    {
        Debug.Log($"Entering the {houseName}");

        SceneStateManager.SaveAndLoadNewScene(targetSceneBuildIndex);
    }
}
