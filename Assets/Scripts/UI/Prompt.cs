using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class Prompt : MonoBehaviour
{
    public Button background, cancel, confirm;

    public delegate void OnButtonClicked();
    public event OnButtonClicked onCanceled, onConfirmed;

    private void Start()
    {
        var cancelAnimated = new UnityAction(() =>
        {
            LeanTween.scale((RectTransform)transform, Vector3.zero, SceneTransitionFader.sceneTransitionOutTime)
            .setOnComplete(() =>
            {
                onCanceled.Invoke();
            });
        });

        background.onClick.AddListener(cancelAnimated);
        cancel.onClick.AddListener(cancelAnimated);
        confirm.onClick.AddListener(() => onConfirmed.Invoke());

        transform.localScale = Vector3.zero;
        LeanTween.scale((RectTransform)transform, Vector3.one, SceneTransitionFader.sceneTransitionOutTime);
    }
}
