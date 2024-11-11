using UnityEngine;

public class StaticObjects : MonoBehaviour
{
    public Transform uiParent, interactionTriggerParent;
    public Transform[] guards;

    public static Transform UiParent { get; private set; }

    public static Transform InteractionTriggerParent { get; private set; }

    public static Transform[] Guards { get; private set; }

    private void Awake()
    {
        UiParent = uiParent;
        InteractionTriggerParent = interactionTriggerParent;
        Guards = guards;
    }
}
