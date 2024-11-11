using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class InteractionManager : MonoBehaviour
{
    public Transform interactionTriggerParent, interactionOverlayParent;
    public GameObject interactionInfoPanel;
    public TextMeshProUGUI interactionInfoText;

    public static InteractionManager Instance { get; private set; }

    List<InteractableObject> interactableObjects;

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
        interactableObjects = new List<InteractableObject>();
        for (int i = 0; i < interactionTriggerParent.childCount; i++)
        {
            var obj = interactionTriggerParent.GetChild(i).GetComponent<InteractableObject>();
            if (obj == null) continue;

            interactableObjects.Add(obj);
        }
    }

    private void Update()
    {
        float closestDistance = -1;
        InteractableObject closestObject = null;

        // TODO: might cause lag, put off in coroutine
        foreach (var obj in interactableObjects)
        {
            float distance = (obj.transform.position - transform.position).magnitude;

            if (distance <= obj.interactionRadius && (distance < closestDistance || closestDistance == -1))
            {
                // object is in range
                closestDistance = distance;
                closestObject = obj;
            }
        }

        // return and hide panel if no object is close
        if (closestObject == null)
        {
            if (interactionInfoPanel.activeSelf)
            {
                interactionInfoPanel.SetActive(false);
            }

            return;
        }

        // display interaction info panel
        interactionInfoText.text = $"Press [f] to {closestObject.interactionMessage}.";
        interactionInfoPanel.SetActive(true);

        // trigger interaction when pressing f
        if (Input.GetKeyDown(KeyCode.F))
        {
            PlayerController.Instance.Interact();
            closestObject.Interact(interactionOverlayParent);
        }
    }

    public void RemoveInteractable(InteractableObject interactable)
    {
        interactableObjects.Remove(interactable);
    }
}
