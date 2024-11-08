using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public GameObject messagePlayerPrefab, messageGameCharacterPrefab;
    public Transform chatScrollViewContent;
    public ScrollRect chatScrollRect;
    public float x;

    const float chatScrollDownThreshold = 0.1f;

    private void Start()
    {
        chatScrollRect.verticalNormalizedPosition = 0;
    }

    private void Update()
    {
        x = chatScrollRect.verticalNormalizedPosition;
    }

    public void SendMessage()
    {
        Instantiate(messagePlayerPrefab, chatScrollViewContent);
        if (chatScrollRect.verticalNormalizedPosition < chatScrollDownThreshold)
        {
            // only scroll down further if user is already scrolled down
            UpdateLayout(transform);
            chatScrollRect.verticalNormalizedPosition = 0;
        }
    }

    /// <summary>
    /// Forces the layout of a UI GameObject and all of it's children to update
    /// their positions and sizes.
    /// </summary>
    /// <param name="xform">
    /// The parent transform of the UI GameObject to update the layout of.
    /// </param>
    public static void UpdateLayout(Transform xform)
    {
        Canvas.ForceUpdateCanvases();
        UpdateLayout_Internal(xform);
    }

    private static void UpdateLayout_Internal(Transform xform)
    {
        if (xform == null || xform.Equals(null))
        {
            return;
        }

        // Update children first
        for (int x = 0; x < xform.childCount; ++x)
        {
            UpdateLayout_Internal(xform.GetChild(x));
        }

        // Update any components that might resize UI elements
        foreach (var layout in xform.GetComponents<LayoutGroup>())
        {
            layout.CalculateLayoutInputVertical();
            layout.CalculateLayoutInputHorizontal();
        }
        foreach (var fitter in xform.GetComponents<ContentSizeFitter>())
        {
            fitter.SetLayoutVertical();
            fitter.SetLayoutHorizontal();
        }
    }
}
