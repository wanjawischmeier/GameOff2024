using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerController))]
public class InteractionManager : MonoBehaviour
{
    public Transform interactionOverlayParent;
    public GameObject interactionInfoPanel;
    public TextMeshProUGUI interactionInfoText;
    public Button buttonA, buttonB;

    public static InteractionManager Instance { get; private set; }
    public static bool disableInteractions = false;

    InteractableObject preferredObject = null;

    const float searchIntervall = 0.1f;

    private void Awake()
    {
        // initialize singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (Application.isMobilePlatform && buttonA != null)
        {
            buttonA.onClick.AddListener(TryInteract);
            buttonA.gameObject.SetActive(true);
            buttonB.gameObject.SetActive(true);
        }

        StartCoroutine(SearchForInteractableObjects());
    }

    private void Update()
    {
        // trigger interaction when pressing f
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryInteract();
        }
    }

    public void TryInteract()
    {
        if (preferredObject == null || disableInteractions)
        {
            return;
        }

        PlayerController.Instance.Interact();
        preferredObject.Interact(interactionOverlayParent);
    }

    private IEnumerator SearchForInteractableObjects()
    {
        var interactionTriggerParent = StaticObjects.InteractionTriggerParent;

        while (true)
        {
            float preferredObjectDistance = -1;
            InteractableObject newPreferredObject = null;

            if (disableInteractions)
            {
                interactionInfoPanel.SetActive(false);
                yield return new WaitForSeconds(searchIntervall);
            }

            for (int i = 0; i < interactionTriggerParent.childCount; i++)
            {
                var interactableObject = interactionTriggerParent.GetChild(i).GetComponent<InteractableObject>();

                ItemTrigger itemTrigger = null;
                if (interactableObject.GetType() == typeof(ItemTrigger))
                {
                    itemTrigger = (ItemTrigger)interactableObject;
                }

                // player shouldn't be able to interact with dropped items
                if (itemTrigger != null && itemTrigger.isDropped) continue;

                float distance = (interactableObject.transform.position - transform.position).magnitude;
                bool isInRange = distance <= interactableObject.interactionRadius;
                bool isNewClosest = distance < preferredObjectDistance || preferredObjectDistance == -1;
                bool isSelectedObject = itemTrigger != null && itemTrigger.item == InventoryManager.Instance.selectedItem;

                if (isInRange && isSelectedObject)
                {
                    // prefer item that is currently selected in hotbar
                    preferredObjectDistance = -2;
                    newPreferredObject = interactableObject;
                }
                else if (isInRange && isNewClosest && preferredObjectDistance != -2)
                {
                    // new closest object
                    preferredObjectDistance = distance;
                    newPreferredObject = interactableObject;
                }
            }

            // return and hide panel if no object is close
            if (newPreferredObject == null)
            {
                preferredObject = null;

                if (interactionInfoPanel.activeSelf)
                {
                    interactionInfoPanel.SetActive(false);
                }
            }
            else if (newPreferredObject != preferredObject && !disableInteractions)
            {
                // display interaction info panel
                preferredObject = newPreferredObject;
                string interactionKey = Application.isMobilePlatform ? "A" : "[f]";
                interactionInfoText.text = $"Press {interactionKey} to {preferredObject.interactionMessage}";
                interactionInfoPanel.SetActive(true);
            }

            yield return new WaitForSeconds(searchIntervall);
        }
    }
}
