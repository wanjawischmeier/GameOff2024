using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoorTrigger : InteractableObject
{
    public override float interactionRadius { get; } = 1.5f;

    public override string interactionMessage { get; } = "open the door and escape";

    public int targetSceneBuildIndex;

    public override void Interact(Transform interactionOverlayParent)
    {
        Debug.Log("Exiting Level");
        SceneManager.LoadScene(targetSceneBuildIndex);
    }
}
