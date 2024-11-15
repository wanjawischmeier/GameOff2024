using UnityEngine;

public class CollisionTigger : MonoBehaviour
{
    public int targetSceneBuildIndex;
    public GameObject promptPrefab;
    public Vector3 shift;

    GameObject promptObj;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.transform.position += shift;
        Utility.SetSceneStopped(true, StaticObjects.Guards);
        promptObj = Instantiate(promptPrefab, StaticObjects.UiParent);
        var prompt = promptObj.GetComponent<Prompt>();
        prompt.onCanceled += OnPromptCanceled;
        prompt.onConfirmed += OnPromptConfirmed;

        Debug.Log($"Entered collision trigger, prompting switch to scene {targetSceneBuildIndex}");
    }

    private void OnPromptCanceled()
    {
        Destroy(promptObj);
        Utility.SetSceneStopped(false, StaticObjects.Guards);
    }

    private void OnPromptConfirmed()
    {
        Debug.Log($"Confirmed prompt to switch scene to {targetSceneBuildIndex}");
        SceneStateManager.ResetSceneState();
        SceneTransitionFader.TransitionToScene(targetSceneBuildIndex);
    }
}
