using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionFader : MonoBehaviour
{
    RectTransform fader;
    static SceneTransitionFader instance;

    public const float sceneTransitionInTime = 0.2f;
    public const float sceneTransitionOutTime = 0.1f;
    public const float sceneLoadBufferTime = 0.2f;

    private void Awake()
    {
        // initialize singleton
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        fader = transform.GetChild(0).GetComponent<RectTransform>();
        fader.gameObject.SetActive(true);

        StartCoroutine(FadeInAferDelay(sceneLoadBufferTime));
    }

    private IEnumerator FadeInAferDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        LeanTween.scale((RectTransform)fader.GetChild(0), Vector3.zero, sceneTransitionOutTime);
        LeanTween.alpha(fader, 0, sceneTransitionInTime).setOnComplete(() =>
        {
            fader.gameObject.SetActive(false);
        });
    }

    private static void TransitionToScene(string sceneName, int sceneBuildIndex)
    {
        instance.fader.gameObject.SetActive(true);

        LeanTween.scale((RectTransform)instance.fader.GetChild(0), Vector3.one, sceneTransitionOutTime);
        LeanTween.alpha(instance.fader, 1, sceneTransitionOutTime).setOnComplete(() =>
        {
            if (sceneBuildIndex == -1)
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                SceneManager.LoadScene(sceneBuildIndex);
            }
        });
    }

    public static void TransitionToScene(int sceneBuildIndex) => TransitionToScene("", sceneBuildIndex);

    public static void TransitionToScene(string sceneName) => TransitionToScene(sceneName, -1);

    public static void ClearStateAndTransitionToChat()
    {
        SceneStateManager.ResetSceneState();
        SceneStateManager.ResetInventoryState();
        StoryStateManager.RemoveNewCollectedItems();

        Cursor.lockState = CursorLockMode.None;
        TransitionToScene("ChatWindow", -1);
    }
}
