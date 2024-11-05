using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public Transform player;
    public GameObject interactionInfoPanel;
    public TextMeshProUGUI interactionInfoText;

    InteractableObject[] interactableObjects;

    void Start()
    {
        var interactableObjectsList = new List<InteractableObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var obj = transform.GetChild(i).GetComponent<InteractableObject>();
            if (obj == null) continue;

            interactableObjectsList.Add(obj);
        }

        interactableObjects = interactableObjectsList.ToArray();
    }

    void Update()
    {
        float closestDistance = -1;
        InteractableObject closestObject = null;

        // TODO: might cause lag, put off in coroutine
        foreach (var obj in interactableObjects)
        {
            float distance = (obj.transform.position - player.position).magnitude;

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
            closestObject.Interact();
        }
    }
}
