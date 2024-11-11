using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Prompt : MonoBehaviour
{
    public Button background, cancel, confirm;

    public delegate void OnButtonClicked();
    public event OnButtonClicked onCanceled, onConfirmed;

    private void Start()
    {
        background.onClick.AddListener(() => onCanceled.Invoke());
        cancel.onClick.AddListener(() => onCanceled.Invoke());
        confirm.onClick.AddListener(() => onConfirmed.Invoke());
    }
}
