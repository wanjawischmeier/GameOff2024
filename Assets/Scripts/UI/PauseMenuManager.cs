using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    public RectTransform fill;
    public Image image;
    float alpha;

    private void Start()
    {
        alpha = image.color.a;
    }

    public void ShowMenu()
    {
        image.color = image.color.WithAlpha(0);
        fill.gameObject.SetActive(true);

        LeanTween.scale((RectTransform)fill.GetChild(0), Vector3.one, SceneTransitionFader.sceneTransitionOutTime);
        LeanTween.alpha(fill, alpha, SceneTransitionFader.sceneTransitionOutTime);
    }

    public void HideMenu()
    {
        LeanTween.scale((RectTransform)fill.GetChild(0), Vector3.zero, SceneTransitionFader.sceneTransitionOutTime);
        LeanTween.alpha(fill, 0, SceneTransitionFader.sceneTransitionInTime).setOnComplete(() =>
        {
            fill.gameObject.SetActive(false);
        });
    }

    public void ToggleMenu()
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

    public void OnMainMenu()
    {
        SceneTransitionFader.TransitionToScene("StartMenu");
    }
}
