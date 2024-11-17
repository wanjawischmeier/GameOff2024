using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class Prompt : MonoBehaviour
{
    public Button background, cancel, confirm;

    public delegate void OnButtonClicked();
    public event OnButtonClicked onCanceled, onConfirmed;

    RectTransform contentTransform;
    float alpha;

    private void Start()
    {
        var cancelAnimated = new UnityAction(() =>
        {
            LeanTween.alpha((RectTransform)transform, 0, SceneTransitionFader.sceneTransitionInTime);
            LeanTween.scale(contentTransform, Vector3.zero, SceneTransitionFader.sceneTransitionOutTime)
            .setOnComplete(() =>
            {
                onCanceled.Invoke();
            });
        });

        background.onClick.AddListener(cancelAnimated);
        cancel.onClick.AddListener(cancelAnimated);
        confirm.onClick.AddListener(() => onConfirmed.Invoke());

        alpha = GetComponent<Image>().color.a;
        contentTransform = (RectTransform)transform.GetChild(0);
        contentTransform.localScale = Vector3.zero;
        LeanTween.scale(contentTransform, Vector3.one, SceneTransitionFader.sceneTransitionOutTime);
        LeanTween.alpha((RectTransform)transform, alpha, SceneTransitionFader.sceneTransitionOutTime);
    }
}
