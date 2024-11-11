using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class InteractionManager : MonoBehaviour
{
    public Transform interactionOverlayParent;
    public GameObject interactionInfoPanel;
    public TextMeshProUGUI interactionInfoText;

    public static InteractionManager Instance { get; private set; }

    List<InteractableObject> interactableObjects;
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
        var interactionTriggerParent = StaticObjects.InteractionTriggerParent;
        interactableObjects = new List<InteractableObject>();
        for (int i = 0; i < interactionTriggerParent.childCount; i++)
        {
            var obj = interactionTriggerParent.GetChild(i).GetComponent<InteractableObject>();
            if (obj == null) continue;

            interactableObjects.Add(obj);
        }

        StartCoroutine(SearchForInteractableObjects());
    }

    private void Update()
    {
        // trigger interaction when pressing f
        if (Input.GetKeyDown(KeyCode.F) && preferredObject != null)
        {
            PlayerController.Instance.Interact();
            preferredObject.Interact(interactionOverlayParent);
        }
    }

    private IEnumerator SearchForInteractableObjects()
    {
        while (true)
        {
            float preferredObjectDistance = -1;
            InteractableObject newPreferredObject = null;

            // TODO: might cause lag, put off in coroutine
            foreach (var obj in interactableObjects)
            {
                float distance = (obj.transform.position - transform.position).magnitude;
                bool isInRange = distance <= obj.interactionRadius;
                bool isNewClosest = distance < preferredObjectDistance || preferredObjectDistance == -1;
                bool isSelectedObject = obj.GetType() == typeof(ItemTrigger) && ((ItemTrigger)obj).item == InventoryManager.Instance.selectedItem;

                if (isInRange && isSelectedObject)
                {
                    // prefer item that is currently selected in hotbar
                    preferredObjectDistance = -2;
                    newPreferredObject = obj;
                }
                else if (isInRange && isNewClosest && preferredObjectDistance != -2)
                {
                    // new closest object
                    preferredObjectDistance = distance;
                    newPreferredObject = obj;
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
            else if (newPreferredObject != preferredObject)
            {
                // display interaction info panel
                preferredObject = newPreferredObject;
                interactionInfoText.text = $"Press [f] to {preferredObject.interactionMessage}.";
                interactionInfoPanel.SetActive(true);
            }

            yield return new WaitForSeconds(searchIntervall);
        }
    }

    public void AddInteractable(InteractableObject interactable)
    {
        interactableObjects.Add(interactable);
    }

    public void RemoveInteractable(InteractableObject interactable)
    {
        interactableObjects.Remove(interactable);
    }
}
