using UnityEngine;

public class StaticObjectsCampusOutside : MonoBehaviour
{
    public Transform uiParent;
    public Transform[] guards;

    public static Transform UiParent { get; private set; }

    public static Transform[] Guards { get; private set; }

    private void Awake()
    {
        UiParent = uiParent;
        Guards = guards;
    }
}
