using UnityEngine;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    public RectTransform fill;
    public Image image;
    float alpha;

    const float escapeKeyBufferTime = 1f;

    private void Start()
    {
        alpha = image.color.a;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (fill.gameObject.activeSelf)
            {
                HideMenu();
            }
            else
            {
                ShowMenu();
            }
        }
    }

    public void ShowMenu()
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        fill.gameObject.SetActive(true);

        LeanTween.scale((RectTransform)fill.GetChild(0), Vector3.one, SceneTransitionFader.sceneTransitionOutTime);
        LeanTween.alpha(fill, alpha, SceneTransitionFader.sceneTransitionOutTime);
        SceneStateManager.SetScenePause(true);
    }

    public void HideMenu()
    {
        LeanTween.scale((RectTransform)fill.GetChild(0), Vector3.zero, SceneTransitionFader.sceneTransitionOutTime);
        LeanTween.alpha(fill, 0, SceneTransitionFader.sceneTransitionInTime).setOnComplete(() =>
        {
            fill.gameObject.SetActive(false);
            SceneStateManager.SetScenePause(false);
        });
    }

    public void OnMainMenu()
    {
        SceneTransitionFader.TransitionToScene("StartMenu");
    }
}